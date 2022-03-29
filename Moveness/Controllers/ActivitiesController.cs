using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moveness.DTOS.Enums;
using Moveness.DTOS.RequestObjects;
using Moveness.DTOS.ResponseObjects;
using Moveness.Models;

namespace Moveness.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivitiesController(DatabaseContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Activities
        [HttpGet]
        public async Task<ActionResult<ActivitiesResponse>> GetActivities([FromQuery] bool today)
        {
            var userId = _userManager.GetUserId(HttpContext.User);

            var user = await _context.Users.Include(x => x.Activities).SingleOrDefaultAsync(x => x.Id == userId);
            
            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            List<Activity> activities;

            if (today)
                activities = user.Activities.Where(x => x.Created.Date == DateTime.Today.Date).ToList();
            else
                activities = user.Activities;

            return ActivitiesResponse.From(activities);
        }

        // GET: api/Activities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityResponse>> GetActivity(int id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var activity = await _context.Activities.Include(x => x.User)
                                                    .SingleOrDefaultAsync(x => x.UserId == userId && x.Id == id);

            if (activity == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ActivityNotFound });

            return ActivityResponse.From(activity);
        }

        // PUT: api/Activities/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActivity(int id, PutActivityRequest request)
        {
            if (id != request.Id)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.IdsNotMatching });

            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Activities).Include(x => x.Challenges).SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var activity = user.Activities.FirstOrDefault(x => x.Id == id);

            if (activity == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ActivityNotFound });

            _context.Activities.Attach(activity);

            activity.Name = request.Name;
            activity.ActiveMinutes = request.ActiveMinutes;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityExists(id))
                    return NotFound(new NotFoundResponse { Code = NotFoundCode.ActivityNotFound });
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Activities
        [HttpPost]
        public async Task<ActionResult<int>> PostActivity(PostActivityRequest request)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Activities)
                                           .Include(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Activities)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var activity = new Activity
            {
                ActiveMinutes = request.ActiveMinutes,
                Name = request.Name,
                User = user,
                UserId = user.Id,
                Created = DateTime.UtcNow
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            //Adding this activity to all challenges
            var activeChallenges = user.Challenges.Where(x => x.Challenge.Accepted == true && x.Challenge.EndTime > DateTime.UtcNow);

            foreach (var challenge in activeChallenges)
            {
                challenge.Challenge.Activities.Add(new ActivitityChallange
                {
                    Activity = activity,
                    Challenge = challenge.Challenge,
                    ChallengeId = challenge.ChallengeId,
                    ActivityId = activity.Id
                });
            }

            await _context.SaveChangesAsync();

            return Ok(activity.Id);
        }

        // DELETE: api/Activities/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ActivityResponse>> DeleteActivity(int id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var activity = await _context.Activities.SingleOrDefaultAsync(x => x.UserId == userId && x.Id == id);

            if (activity == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ActivityNotFound });

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool ActivityExists(int id)
        {
            return _context.Activities.Any(e => e.Id == id);
        }
    }
}
