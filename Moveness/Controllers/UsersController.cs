using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moveness.DTOS.Enums;
using Moveness.DTOS.RequestObjects;
using Moveness.DTOS.ResponseObjects;
using Moveness.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Moveness.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public UsersController(DatabaseContext context, 
            UserManager<ApplicationUser> userManager, 
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            return UserResponse.From(user);
        }

        // GET: api/Users/Me
        [HttpGet("Me")]
        public async Task<ActionResult<MeResponse>> GetMyself()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            return MeResponse.From(user);
        }

        // POST: api/Users/Search
        [HttpPost("Search")]
        public async Task<ActionResult<UsersResponse>> PostSearchUser(PostSearchRequest request)
        {
            var userId = _userManager.GetUserId(HttpContext.User);

            var users = await _context.Users.Where(x => (x.UserName.Contains(request.Query) || x.Email == request.Query) && x.Id != userId)
                                            .ToListAsync();

            return UsersResponse.From(users);
        }

        // PUT: api/Users/Me/DeviceToken
        [HttpPut("Me/DeviceToken")]
        public async Task<ActionResult> EditDeviceToken(PutDeviceTokenRequest request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            
            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            user.DeviceToken = request.DeviceToken;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Users/Me/PreferredLanguage
        [HttpPut("Me/PreferredLanguage")]
        public async Task<ActionResult> EditPreferredLanguage(PutPreferredLanguageRequest request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            user.PreferredLanguage = request.PreferredLanguage;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Users/Me
        [HttpPut("Me")]
        public async Task<IActionResult> PutMyself(string id, PutUserRequest request)
        {
            //TODO: Split into difference calls?

            if (id != request.Id)
                return BadRequest(new BadRequestResponse { Code = BadRequestCode.IdsNotMatching });

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            _context.Users.Attach(user);
            
            //TODO Validate that it is a correct image in our container?
            if (request.ProfileImageURL != null)
            {
                user.ProfileImageURL = request.ProfileImageURL;
            }

            if (request.Username != null && user.UserName != request.Username)
            {
                var result = await _userManager.SetUserNameAsync(user, request.Username);

                //If not succeeds then return bad request object
                if (!result.Succeeded)
                {
                    var response = BadRequestResponse.FromIdentity(result.Errors.Select(x => x.Code));
                    return BadRequest(response);
                }
            }

            if (request.Email != null && user.Email != request.Email)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                string callbackUrl = Url.ActionLink(action: "ConfirmEmail", controller: "Account",
                                                    values: new { userId = user.Id, token },
                                                    protocol: Request.Scheme);

                var subject = "Bekräfta e-postadress Motionskampen";
                var message = $"Bekräfta din e-postadress för att bättre kunna använda våra tjänster.\n\n{callbackUrl}";

                await _emailSender.SendEmailAsync(request.Email, subject, message);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Users/Me
        [HttpDelete("Me")]
        public async Task<ActionResult<Team>> DeleteMyself()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var user = await _context.Users.Include(x => x.Activities)
                                           .Include(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Owner)
                                           .Include(x => x.Activities)
                                           .Include(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .ThenInclude(x => x.Users)
                                           .ThenInclude(x => x.User)
                                           .Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Owner)
                                           .Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Users)
                                           .ThenInclude(x => x.User)
                                           .Include(x => x.Teams)
                                           .ThenInclude(x => x.Team)
                                           .ThenInclude(x => x.Challenges)
                                           .ThenInclude(x => x.Challenge)
                                           .SingleOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            //Unreferenceing
            user.Activities.Clear();

            //Deleting teams where user is owner
            var ownerTeams = user.Teams.Where(x => x.Team.Owner == user);

            foreach (var team in ownerTeams)
            {
                if (team.Team.Users.Count > 1)
                {
                    var newOwner = team.Team.Users.Select(x => x.User).FirstOrDefault(x => x.Id != userId);
                    team.Team.Owner = newOwner;

                    foreach (var challenge in team.Team.Challenges)
                        challenge.Challenge.ChallengingUserId = newOwner.Id;
                }
                else
                {
                    team.Team.Users.Clear();
                    _context.Challenges.RemoveRange(team.Team.Challenges.Select(x => x.Challenge));
                    _context.Teams.Remove(team.Team);
                }
            }

            user.Teams.Clear();

            _context.Challenges.RemoveRange(user.Challenges.Select(x => x.Challenge));

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
