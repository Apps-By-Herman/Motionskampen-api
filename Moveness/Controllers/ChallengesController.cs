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
using Moveness.Services;

namespace Moveness.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengesController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPushNotificationService _pushNotification;

        public ChallengesController(DatabaseContext context, UserManager<ApplicationUser> userManager, IPushNotificationService pushNotification)
        {
            _context = context;
            _userManager = userManager;
            _pushNotification = pushNotification;
        }

        // GET: api/Challenges
        [HttpGet]
        public async Task<ActionResult<ChallengesResponse>> GetChallenges(bool accepted)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Users)
                                           .Include(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Activities)
                                           .ThenInclude(x => x.Activity)
                                           .ThenInclude(x => x.User)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            List<ApplicationUserChallenge> challenges;

            if (accepted)
                challenges = user.Challenges.Where(x => x.Challenge.Accepted == true && x.Challenge.EndTime > DateTime.UtcNow).ToList();
            else
                challenges = user.Challenges.Where(x => (x.Challenge.Accepted == null && x.Challenge.Owner != user)
                                                        || (x.Challenge.Accepted == true && x.Challenge.EndTime > DateTime.UtcNow))
                                            .OrderBy(x => x.Challenge.Accepted)
                                            .ToList();

            return await ChallengesResponse.From(challenges, _context);
        }


        // GET: api/Challenges/Finished
        [HttpGet("Finished")]
        public async Task<ActionResult<ChallengesResponse>> GetFinishedChallenges()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Users)
                                           .Include(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Activities)
                                           .ThenInclude(x => x.Activity)
                                           .ThenInclude(x => x.User)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var challenges = user.Challenges.Where(x => x.Challenge.EndTime < DateTime.UtcNow
                                                        && x.Challenge.Accepted == true).ToList();

            return await ChallengesResponse.From(challenges, _context);
        }

        // GET: api/Challenges/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChallengeResponse>> GetChallenge(int id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Challenges)
                                                       .ThenInclude(x => x.Challenge)
                                                       .ThenInclude(x => x.Users)
                                                       .Include(x => x.Challenges)
                                                       .ThenInclude(x => x.Challenge)
                                                       .ThenInclude(x => x.Activities)
                                                       .ThenInclude(x => x.Activity)
                                                       .ThenInclude(x => x.User)
                                                       .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var applicationUserChallenge = user.Challenges.SingleOrDefault(x => x.ChallengeId == id);

            if (applicationUserChallenge == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengeNotFound });

            var challenge = applicationUserChallenge.Challenge;

            if (challenge.IsTeamChallenge)
            {
                var challengingTeam = await _context.Teams.Include(x => x.Users)
                                                          .FirstOrDefaultAsync(x => x.Id == challenge.ChallengingTeamId);

                var challengedTeam = await _context.Teams.Include(x => x.Users)
                                                         .FirstOrDefaultAsync(x => x.Id == challenge.ChallengedTeamId);
                
                return ChallengeResponse.From(challenge, challengingTeam, challengedTeam);
            }

            var challengingUser = await _context.Users.FindAsync(challenge.ChallengingUserId);
            var challengedUser = await _context.Users.FindAsync(challenge.ChallengedUserId);

            return ChallengeResponse.From(challenge, challengingUser, challengedUser);
        }

        // PUT: api/Challenges/5/Reply
        [HttpPut("{id}/Reply")]
        public async Task<ActionResult<Challenge>> PutReplyChallenge(int id, PostReplyChallengeRequest request)
        {
            if (id != request.Id)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.IdsNotMatching });

            var userId = _userManager.GetUserId(HttpContext.User);

            var challenge = await _context.Challenges.SingleOrDefaultAsync(x => x.Id == id && x.ChallengedUserId == userId);

            if (challenge == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengeNotFound });

            _context.Challenges.Attach(challenge);

            challenge.Accepted = request.Accepted;

            if (challenge.Accepted == true)
            {
                challenge.StartTime = DateTime.UtcNow.Date;
                challenge.EndTime = DateTime.UtcNow.AddDays(15).Date;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChallengeExists(id))
                    return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengeNotFound });
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Challenges/User
        [HttpPost("User")]
        public async Task<ActionResult<int>> PostUserChallenge(PostUserChallengeRequest request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            if (request.ChallengedId == user.Id)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.CanNotChallengeYourself });

            var challengedUser = await _context.Users.FindAsync(request.ChallengedId);

            if (challengedUser == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengedUserNotFound });

            var challenge = new Challenge
            {
                ChallengedUserId = challengedUser.Id,
                Message = $"{user.UserName} challenges {challengedUser.UserName} on Motionskampen!",
                ChallengingUserId = user.Id,
                Owner = user,
                IsTeamChallenge = false,
                Users = new List<ApplicationUserChallenge>()
            };

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            challenge.Users.Add(new ApplicationUserChallenge { Challenge = challenge, ChallengeId = challenge.Id, User = user, UserId = user.Id });
            challenge.Users.Add(new ApplicationUserChallenge { Challenge = challenge, ChallengeId = challenge.Id, User = challengedUser, UserId = challengedUser.Id });

            await _context.SaveChangesAsync();

            await _pushNotification.NewPrivateChallenge(challengedUser);

            return Ok(challenge.Id);
        }

        // POST: api/Challenges/User/Random
        [HttpPost("User/Random")]
        public async Task<ActionResult<int>> PostRandomUserChallenge()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            //Randomly get a challengedUser
            var challengedUser = await _context.Users.Where(x => x != user).OrderBy(c => Guid.NewGuid()).FirstOrDefaultAsync();

            if (challengedUser == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengedUserNotFound });

            var challenge = new Challenge
            {
                ChallengedUserId = challengedUser.Id,
                Message = $"{user.UserName} challenges {challengedUser.UserName} on Motionskampen!",
                ChallengingUserId = user.Id,
                Owner = user,
                IsTeamChallenge = false,
                Users = new List<ApplicationUserChallenge>()
            };

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            challenge.Users.Add(new ApplicationUserChallenge { Challenge = challenge, ChallengeId = challenge.Id, User = user, UserId = user.Id });
            challenge.Users.Add(new ApplicationUserChallenge { Challenge = challenge, ChallengeId = challenge.Id, User = challengedUser, UserId = challengedUser.Id });

            await _context.SaveChangesAsync();

            await _pushNotification.NewPrivateChallenge(challengedUser);

            return Ok(challenge.Id);
        }

        // POST: api/Challenges/Team
        [HttpPost("Team")]
        public async Task<ActionResult<Challenge>> PostTeamChallenge(PostTeamChallengeRequest request)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Users)
                                           .Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Owner)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var challengingTeam = user.Teams.SingleOrDefault(x => x.TeamId == request.ChallengingTeamId);

            if (challengingTeam == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengingTeamNotFound });

            if (challengingTeam.Team.Owner != user)
                return Unauthorized(new UnauthorizedResponse { Code = UnauthorizedCode.UserNotOwnerOfTeam });

            var challengedTeam = await _context.Teams.Include(x => x.Owner)
                                                     .Include(x => x.Users)
                                                     .SingleOrDefaultAsync(x => x.Id == request.ChallengedTeamId);

            if (challengedTeam == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengedTeamNotFound });

            if (challengingTeam.TeamId == challengedTeam.Id)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.CanNotChallengeYourself });

            if (challengedTeam.Owner == user)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.CanNotChallengeYourself });

            var challenge = new Challenge
            {
                Owner = user,
                Message = $"{challengingTeam.Team.Name} challenges {challengedTeam.Name} on Motionskampen!", 
                IsTeamChallenge = true,
                ChallengingUserId = user.Id,
                ChallengingTeamId = challengingTeam.TeamId,
                ChallengedUserId = challengedTeam.Owner.Id,
                ChallengedTeamId = challengedTeam.Id,
                Users = new List<ApplicationUserChallenge>(),
                Teams = new List<TeamChallenge>(),
            };

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            foreach (var applicationUserTeam in challengedTeam.Users)
            {
                challenge.Users.Add(new ApplicationUserChallenge 
                { 
                    Challenge = challenge, 
                    ChallengeId = challenge.Id, 
                    User = applicationUserTeam.User, 
                    UserId = applicationUserTeam.UserId 
                });
            }

            foreach (var applicationUserTeam in challengingTeam.Team.Users)
            {
                challenge.Users.Add(new ApplicationUserChallenge
                {
                    Challenge = challenge,
                    ChallengeId = challenge.Id,
                    User = applicationUserTeam.User,
                    UserId = applicationUserTeam.UserId
                });
            }

            challenge.Teams.Add(new TeamChallenge { Challenge = challenge, ChallengeId = challenge.Id, Team = challengingTeam.Team, TeamId = challengingTeam.TeamId });
            challenge.Teams.Add(new TeamChallenge { Challenge = challenge, ChallengeId = challenge.Id, Team = challengedTeam, TeamId = challengedTeam.Id });

            await _context.SaveChangesAsync();

            await _pushNotification.NewTeamChallenge(challengedTeam, challengingTeam.Team, challengedTeam.Owner);

            return Ok(challenge.Id);
        }

        // POST: api/Challenges/Team/Random
        [HttpPost("Team/Random")]
        public async Task<ActionResult<Challenge>> PostRandomTeamChallenge(PostRandomTeamChallengeRequest request)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Owner)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var userTeamIds = user.Teams.Select(x => x.TeamId);

            var challengingTeam = user.Teams.SingleOrDefault(x => x.TeamId == request.ChallengingTeamId);

            if (challengingTeam == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengingTeamNotFound });

            if (challengingTeam.Team.Owner != user)
                return Unauthorized(new UnauthorizedResponse { Code = UnauthorizedCode.UserNotOwnerOfTeam });

            var challengedTeam = await _context.Teams.Include(x => x.Owner)
                                                     .Where(x => !userTeamIds.Contains(x.Id))
                                                     .OrderBy(c => Guid.NewGuid())
                                                     .FirstOrDefaultAsync();

            if (challengedTeam == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengedTeamNotFound });

            if (challengingTeam.TeamId == challengedTeam.Id)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.CanNotChallengeYourself });

            if (challengedTeam.Owner == user)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.CanNotChallengeYourself });

            var challenge = new Challenge
            {
                Owner = user,
                Message = $"{challengingTeam.Team.Name} challenges {challengedTeam.Name} on Motionskampen!",
                IsTeamChallenge = true,
                ChallengingUserId = user.Id,
                ChallengingTeamId = challengingTeam.TeamId,
                ChallengedUserId = challengedTeam.Owner.Id,
                ChallengedTeamId = challengedTeam.Id,
                Users = new List<ApplicationUserChallenge>(),
                Teams = new List<TeamChallenge>(),
            };

            _context.Challenges.Add(challenge);
            await _context.SaveChangesAsync();

            challenge.Users.Add(new ApplicationUserChallenge { Challenge = challenge, ChallengeId = challenge.Id, User = user, UserId = user.Id });
            challenge.Users.Add(new ApplicationUserChallenge { Challenge = challenge, ChallengeId = challenge.Id, User = challengedTeam.Owner, UserId = challengedTeam.Owner.Id });

            challenge.Teams.Add(new TeamChallenge { Challenge = challenge, ChallengeId = challenge.Id, Team = challengingTeam.Team, TeamId = challengingTeam.TeamId });
            challenge.Teams.Add(new TeamChallenge { Challenge = challenge, ChallengeId = challenge.Id, Team = challengedTeam, TeamId = challengedTeam.Id });

            await _context.SaveChangesAsync();

            await _pushNotification.NewTeamChallenge(challengedTeam, challengingTeam.Team, challengedTeam.Owner);

            return Ok(challenge.Id);
        }

        // DELETE: api/Challenges/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Challenge>> DeleteChallenge(int id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var applicationUserChallenge = user.Challenges.SingleOrDefault(x => x.ChallengeId == id);

            if (applicationUserChallenge == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.ChallengeNotFound });

            var challenge = applicationUserChallenge.Challenge;

            if (challenge.Owner != user)
                return Unauthorized(new UnauthorizedResponse { Code = UnauthorizedCode.UserNotOwnerOfChallenge });

            if (challenge.Accepted == true)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.CanNotDeleteAcceptedChallenge });

            user.Challenges.Remove(applicationUserChallenge);
            _context.Challenges.Remove(applicationUserChallenge.Challenge);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool ChallengeExists(int id)
        {
            return _context.Challenges.Any(e => e.Id == id);
        }
    }
}
