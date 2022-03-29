using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moveness.DTOS.Enums;
using Moveness.DTOS.RequestObjects;
using Moveness.DTOS.ResponseObjects;
using Moveness.Models;
using Moveness.Services;

namespace Moveness.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBlobService _blobService;
        private readonly IPushNotificationService _pushNotification;

        public TeamsController(DatabaseContext context,
                                UserManager<ApplicationUser> userManager,
                                IBlobService blobService,
                                IPushNotificationService pushNotification)
        {
            _context = context;
            _userManager = userManager;
            _blobService = blobService;
            _pushNotification = pushNotification;
        }

        // GET: api/Teams
        [HttpGet]
        public async Task<ActionResult<TeamsResponse>> GetTeams()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Users)
                                           .ThenInclude(x => x.User)
                                           .Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Activities)
                                           .ThenInclude(x => x.Activity)
                                           .ThenInclude(x => x.User)
                                           .Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Users)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            return TeamsResponse.From(user.Teams);
        }

        // GET: api/Teams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TeamResponse>> GetTeam(int id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Users)
                                           .ThenInclude(x => x.User)
                                           .Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Activities)
                                           .ThenInclude(x => x.Activity)
                                           .ThenInclude(x => x.User)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var applicationUserTeam = user.Teams.FirstOrDefault(x => x.TeamId == id);

            if (applicationUserTeam == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamNotFound });

            return await TeamResponse.From(applicationUserTeam, _context);
        }

        // PUT: api/Teams/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTeam(int id, PutTeamRequest request)
        {
            if (id != request.Id)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.IdsNotMatching });

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var team = await _context.Teams.FindAsync(id);

            if (team == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamNotFound });

            //Only owner of team can edit team.
            if (team.Owner != user)
                return Unauthorized(new UnauthorizedResponse { Code = UnauthorizedCode.UserNotOwnerOfTeam });

            _context.Teams.Attach(team);

            team.Name = request.Name;
            team.Description = request.Description;
            team.ImageURL = request.ImageURL;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamExists(id))
                    return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamNotFound });
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Teams
        [HttpPost]
        public async Task<ActionResult<Team>> PostTeam(PostTeamRequest request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var team = new Team
            {
                Description = request.Description,
                Name = request.Name,
                Owner = user,
                Users = new List<ApplicationUserTeam>()
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            /*
            if (teamImage != null)
                team.ImageURL = await _blobService.UploadImageTeam(null);
            */

            team.Users.Add(new ApplicationUserTeam { Team = team, TeamId = team.Id, User = user, UserId = user.Id });

            var users = _context.Users.Where(x => request.UserIds.Contains(x.Id));

            foreach (var u in users)
            {
                team.Users.Add(new ApplicationUserTeam { Team = team, TeamId = team.Id, User = u, UserId = u.Id });
                await _pushNotification.AddedToTeam(team.Name, u);
            }

            await _context.SaveChangesAsync();

            return Ok(team.Id);
        }

        // POST: api/Teams/Search
        [HttpPost("Search")]
        public async Task<ActionResult<TeamsResponse>> PostSearchTeam(PostSearchRequest request)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Teams)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var userTeamIds = user.Teams.Select(x => x.TeamId);

            var teams = await _context.Teams.Include(x => x.Users)
                                            .ThenInclude(x => x.User)
                                            .Where(x => x.Name.Contains(request.Query)
                                                        && !userTeamIds.Contains(x.Id))
                                            .ToListAsync();

            return TeamsResponse.From(teams, user);
        }

        // POST: api/Teams/5/Members
        [HttpPost("{id}/Members")]
        public async Task<ActionResult> PostAddTeamMember(int id, PostAddTeamMemberRequest request)
        {
            if (id != request.TeamId)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.IdsNotMatching });

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var team = await _context.Teams.Include(x => x.Users).SingleOrDefaultAsync(x => x.Id == request.TeamId);

            if (team == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamNotFound });

            //Only owner of team can add team member.
            if (team.Owner != user)
                return Unauthorized(new UnauthorizedResponse { Code = UnauthorizedCode.UserNotOwnerOfTeam });

            foreach (var uId in request.UserIds)
            {
                var newMember = await _context.Users.FindAsync(uId);

                if (newMember == null)
                    return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamMemberNotFound });

                if (team.Users.Select(x => x.UserId).Contains(newMember.Id))
                    return BadRequest(new BadRequestResponse { Code = BadRequestCode.UserAlreadyInTeam });

                var applicationUserTeam = new ApplicationUserTeam { Team = team, TeamId = team.Id, User = newMember, UserId = newMember.Id };

                if (team.Users.Contains(applicationUserTeam))
                    return BadRequest(new BadRequestResponse { Code = BadRequestCode.UserAlreadyInTeam });

                team.Users.Add(applicationUserTeam);

                await _context.SaveChangesAsync();
                await _pushNotification.AddedToTeam(team.Name, newMember);
            }

            return Ok();
        }

        // POST: api/Teams/5/Leave
        [HttpPost("{id}/Leave")]
        public async Task<ActionResult> PostLeaveTeam(int id, PostLeaveTeamRequest request)
        {
            if (id != request.TeamId)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.IdsNotMatching });

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var team = await _context.Teams.Include(x => x.Users)
                                           .ThenInclude(x => x.User)
                                           .SingleOrDefaultAsync(x => x.Id == request.TeamId);

            if (team == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamNotFound });

            var applicationUserTeam = team.Users.FirstOrDefault(x => x.User == user);

            if (applicationUserTeam == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamMemberNotFound });

            //Owner of team cant leave team.
            if (team.Owner == user)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.UserIsOwnerOfTeam });

            team.Users.Remove(applicationUserTeam);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Teams/5/Members/5
        [HttpDelete("{teamId}/Members/{id}")]
        public async Task<ActionResult<Team>> DeleteTeamMember(string id, int teamId, DeleteUserFromTeamRequest request)
        {
            if (id != request.Id)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.IdsNotMatching });

            if (teamId != request.TeamId)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.IdsNotMatching });

            var team = await _context.Teams.Include(x => x.Users)
                                           .SingleOrDefaultAsync(x => x.Id == request.TeamId);

            if (team == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamNotFound });

            var user = await _userManager.GetUserAsync(HttpContext.User);

            //Only owner of team can add team member.
            if (team.Owner != user)
                return Unauthorized(new UnauthorizedResponse { Code = UnauthorizedCode.UserNotOwnerOfTeam });

            var teamMember = await _context.Users.FindAsync(request.Id);

            if (teamMember == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamMemberNotFound });

            var applicationUserTeam = team.Users.FirstOrDefault(x => x.User == teamMember);

            if (applicationUserTeam == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamMemberNotFound });

            team.Users.Remove(applicationUserTeam);

            await _context.SaveChangesAsync();

            await _pushNotification.RemovedFromTeam(team.Name, teamMember);

            return Ok();
        }

        // DELETE: api/Teams/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Team>> DeleteTeam(int id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Owner)
                                           .Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Users)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var applicationUserTeam = user.Teams.FirstOrDefault(x => x.TeamId == id);

            if (applicationUserTeam == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.TeamNotFound });

            //Only owner of team can remove team.
            if (applicationUserTeam.Team.Owner != user)
                return Unauthorized(new UnauthorizedResponse { Code = UnauthorizedCode.UserNotOwnerOfTeam });

            //Not allowed to remove if any challenges is ongoing
            if (applicationUserTeam.Team.Challenges.Any(x => x.Challenge.StartTime < DateTime.UtcNow && x.Challenge.EndTime > DateTime.UtcNow))
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.CanNotDeleteTeamWhenActiveChallenges });

            //Clear challenges
            foreach (var challenge in applicationUserTeam.Team.Challenges)
                _context.Challenges.Remove(challenge.Challenge);

            //Clear members of team
            foreach (var aUT in applicationUserTeam.Team.Users)
                _context.ApplicationUserTeam.Remove(aUT);

            //Remove team
            _context.Teams.Remove(applicationUserTeam.Team);
            
            //Save everything
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }
    }
}
