
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SSAPI.ViewModel;
using SSBOL;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
  public class AccountsController:ControllerBase // AccountsController is inherting from controllerBase class
    
    {
        UserManager<SSUser> userManager;  // 
        SignInManager<SSUser> signInManager;
        public AccountsController(SignInManager<SSUser> _signInManager, UserManager<SSUser> _userManager)
        {
            signInManager = _signInManager;
            userManager = _userManager;

        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new SSUser()
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        DOB = model.DOB
                    };
                    var userResult = await userManager.CreateAsync(user, model.Password);
                    if (userResult.Succeeded)
                    {
                        var roleResult = await userManager.AddToRoleAsync(user, "User"); // we are assigning a user to userrole
                        if (roleResult.Succeeded)
                        {
                            return Ok(user); // we are returing the user we just created from the database
                        }
                    }
                    else
                    {
                        foreach (var item in userResult.Errors)
                        {
                            ModelState.AddModelError(item.Code, item.Description); // if doesnt work we returing the issue
                        }
                    }
                }
                return BadRequest(ModelState);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error! Please Contact Admin"); //related to server issue 500
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            { 
                if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.UserName);
                var signInResult = await signInManager.CheckPasswordSignInAsync(user, model.Password,true);
                if (signInResult.Succeeded)
                {
                  
                    var roles = await userManager.GetRolesAsync(user);
                        // Step 1 creating Claims
                        IdentityOptions identityOptions = new IdentityOptions();
                        var claims = new Claim[]
                        {
                            new Claim(identityOptions.ClaimsIdentity.UserIdClaimType,user.Id),
                            new Claim(identityOptions.ClaimsIdentity.UserIdClaimType,user.UserName),
                            new Claim(identityOptions.ClaimsIdentity.RoleClaimType,roles[0])
                        };
                        // step 2 Create signInKey from secretKey
                        var signingkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("This - is my-secure-code-for-jwt-authentication-phrase"));
                        // step 3: create signing credentials from signingkey with HMAC alogorithm
                        var signingCredentials = new SigningCredentials(signingkey, SecurityAlgorithms.HmacSha256);
                        // step 4 Create JWT with signingCredentials, identityClaims and expire duration
                        var jwt = new JwtSecurityToken(signingCredentials: signingCredentials,
                            expires: DateTime.Now.AddMinutes(30), claims: claims);
                        // step 5 finally write the token as response with(OK)
                        return Ok(new { tokenJWT = new JwtSecurityTokenHandler().WriteToken(jwt), id = user.Id, username = user.UserName, role = roles[0] });
                }
                else
                {
                    ModelState.AddModelError("", "Invalid UserName or Password");
                }
            }
            return BadRequest(ModelState);

            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error! Please Contact Admin!");
            }
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return NoContent();
        }
    }
}
