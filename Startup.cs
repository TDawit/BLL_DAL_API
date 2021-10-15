using DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SSBOL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
           
            services.AddSwaggerDocument(); // to use swagger

            services.AddTransient<IStoryDb, StoryDb>();   // Istory is userdefiend we created a reference Istorydb dependecy injection 

            //TRANSIENT will create one object storydb per request
            

            services.ConfigureApplicationCookie(opt =>
            {
                opt.Events = new CookieAuthenticationEvents()
                {
                    //Authentication
                    OnRedirectToLogin = RedirectContext =>
                    {
                        RedirectContext.HttpContext.Response.StatusCode = 403; // forbidden
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = RedirectContext =>
                    {
                        RedirectContext.HttpContext.Response.StatusCode = 401;//UnAuthorized
                        return Task.CompletedTask;
                    }
                };

            });
            // step 1 create siginkey from secretkey
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("This - is my-secure-code-for-jwt-authentication-phrase"));
            // step 2 create validation Parameters using signingkey
            var tokenValidationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = signingKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
            // step 3 set authentication Type as JWTBearer
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

            })
                // step 4 set validation parameters created above
                .AddJwtBearer(Jwt =>
                {
                    Jwt.TokenValidationParameters = tokenValidationParameters; // small t
                });
            // aplying polciy application level
            var authPol = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(new string[]
                {
                    JwtBearerDefaults.AuthenticationScheme })
                .RequireAuthenticatedUser()
                .Build();
            services.AddControllers(
           config => config.Filters.Add(new AuthorizeFilter(authPol) // authorization is appliad at application level
           ));
            services.AddDbContext<SSDbContext>();  // this dependecy injection we are passing the object
            services.AddIdentity<SSUser, IdentityRole>() //IdentityRole is a predefined 
                    .AddEntityFrameworkStores<SSDbContext>() //SSDBObect is the obect name 
                .AddDefaultTokenProviders();






        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseOpenApi(); // swagger
                app.UseSwaggerUi3(); // for swagger
            }
            app.UseCors(x => x.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                
                
                    endpoints.MapControllers();
            });
        }
    }
}
