using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSBOL;


namespace SSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoriesController : Controller  // stories controller is inherating from ControllerBase class
    {
        IStoryDb storyDb;  // we are using repository pattern custom object

        public StoriesController(IStoryDb _storyDb) //Istory is the object
        {
            storyDb = _storyDb;
        }

        [HttpGet]
        public async Task<IActionResult> GetStories()
        {
            var strs = await storyDb.GetAll().ToListAsync();
            return Ok(strs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStoriesById(int id)
        {
            var str = await storyDb.GetById(id).FirstOrDefaultAsync();
            return Ok(str);
        }

        [HttpGet("getStoriesByUserId/{id}")]
        public async Task<IActionResult> GetStoriesByUserId(string id)
        {
            var str = await storyDb.GetByUserId(id).ToListAsync();
            return Ok(str);
        }

        [HttpGet("getStoriesByStatus/{isApproved}")]
        public async Task<IActionResult> GetStoriesByStatus(bool isApproved)
        {
            var strs = await storyDb.GetAll(isApproved).ToListAsync();
            return Ok(strs);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStory(int id)
        {
            var result = await storyDb.Delete(id);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PostStory(Story story)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    story.CreatedOn = DateTime.Now;
                    if (User.IsInRole("Admin"))
                    {
                        story.IsApproved = true;
                    }
                    else
                    {
                        story.IsApproved = false;
                    }
                    story.Id = User.FindFirst(ClaimTypes.NameIdentifier).Value; //User.Claims.FirstOrDefault().Value
                    var result = await storyDb.Create(story);
                    return CreatedAtAction("GetStoriesById", new { id = story.SSId }, story);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception E)
            {
                //E
                var msg = (E.InnerException != null) ? (E.InnerException.Message) : (E.Message);
                //Log in some file...
                return StatusCode(500, "Admin is working on it! " + msg);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutStory(int id, Story story)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (storyDb.GetById(id) != null)
                    {
                        var result = await storyDb.Update(story);
                        return NoContent();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception E)
            {
                //E
                var msg = (E.InnerException != null) ? (E.InnerException.Message) : (E.Message);
                return StatusCode(500, "Admin is working on it! " + msg);
            }
        }

         [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("approveStory/{id}")]
        public async Task<IActionResult> ApproveStory(int id, Story story)
        {
            try
            {
                if (storyDb.GetById(id) != null)
                {
                    await storyDb.Approve(id);
                    return NoContent();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception E)
            {
                //E
                var msg = (E.InnerException != null) ? (E.InnerException.Message) : (E.Message);
                return StatusCode(500, "Admin is working on it! " + msg);
            }
        }

    }
}

