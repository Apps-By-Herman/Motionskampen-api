using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Moveness.DTOS.Enums;
using Moveness.DTOS.RequestObjects;
using Moveness.DTOS.ResponseObjects;
using Moveness.Helpers;
using Moveness.Models;
using Moveness.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moveness.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly DatabaseContext _context;
        private readonly ITokenService _accountService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IBlobService _blobService;

        public AccountController(SignInManager<ApplicationUser> signInManager,
                                    DatabaseContext context,
                                    ITokenService accountService,
                                    UserManager<ApplicationUser> userManager,
                                    IEmailSender emailSender,
                                    IBlobService blobService)
        {
            _accountService = accountService;
            _signInManager = signInManager;
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _blobService = blobService;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<TokenResponse>> Login()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues credentialsValues);
            string[] credentials = GetUserNameAndPassword(credentialsValues);
            var username = credentials.FirstOrDefault();
            var password = credentials.LastOrDefault();

            var user = await _userManager.FindByEmailAsync(username);

            if (user == null)
                user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return Unauthorized();

            var result = await _signInManager.PasswordSignInAsync(user, password, true, false);

            if (!result.Succeeded)
                return Unauthorized();

            var authorizationTokens = _accountService.CreateAccessToken(user.Id, user.UserName);
            return Ok(authorizationTokens);
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            //Create user
            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
            };

            //Create in user manager
            var result = await _userManager.CreateAsync(user, request.Password);

            //If not succeeds then return bad request object
            if (!result.Succeeded)
                return BadRequest(BadRequestResponse.FromIdentity(result.Errors.Select(x => x.Code)));

            user.ProfileImageURL = await _blobService.UploadImageUser(null);

            await _userManager.UpdateAsync(user);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            string callbackUrl = Url.ActionLink(action: "ConfirmEmail", controller: "Account",
                                                values: new { userId = user.Id, token },
                                                protocol: Request.Scheme);

            string subject = user.PreferredLanguage == "sv" ? "Bekräfta e-postadress Motionskampen" : "Confirm email Motionskampen";
            string message = user.PreferredLanguage == "sv" 
                ? $"Bekräfta din e-postadress för att bättre kunna använda våra tjänster.\n\n{callbackUrl}" 
                : $"Confirm your email to be able to use all of our services.\n\n{callbackUrl}";

            await _emailSender.SendEmailAsync(request.Email, subject, message);

            var singInResult = await _signInManager.PasswordSignInAsync(user, request.Password, true, false);

            if (!singInResult.Succeeded)
                return Ok();

            var authorizationTokens = _accountService.CreateAccessToken(user.Id, user.UserName);

            return Ok(authorizationTokens);
        }

        [HttpGet("ConfirmEmail")]
        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                NotFound(user.PreferredLanguage == "sv" ? "Användaren hittades inte." : "User not found.");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return BadRequest(user.PreferredLanguage == "sv" ? "Misslyckades! Inte en giltig token." : "Failed! Token not valid.");

            return Ok(user.PreferredLanguage == "sv" ? "Lyckades! Din e-postadress är nu bekräftad." : "Success! Your email is now confirmed.");
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return Ok(); // We dont want to give away if there is a user with this email.

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string title = user.PreferredLanguage == "sv" ? "Återställ lösenord Motionskampen" : "Reset password Motionskampen";
            string message = user.PreferredLanguage == "sv" ? $"Kopiera koden in i appen.\n\n{token}" : $"Copy following code into app.\n\n{token}";

            await _emailSender.SendEmailAsync(user.Email, title, message);

            return Ok();
        }

        [HttpPost("ConfirmResetPassword")]
        public async Task<ActionResult> ConfirmResetPassword(ConfirmResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return NotFound(new NotFoundResponse { Code = NotFoundCode.UserNotFound });

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

            if (!result.Succeeded)
                BadRequest(BadRequestResponse.FromIdentity(result.Errors.Select(x => x.Code)));

            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("RenewAccessToken")]
        public ActionResult RenewAccessToken()
        {
            var username = _userManager.GetUserName(HttpContext.User);
            var userId = _userManager.GetUserId(HttpContext.User);

            var authorizationTokens = _accountService.CreateAccessToken(userId, username);
            return Ok(authorizationTokens);
        }

        private static string[] GetUserNameAndPassword(StringValues credentialsValues)
        {
            string credentialsBase64 = credentialsValues.ToString().TrimStart("Basic".ToCharArray()).TrimStart();

            if (IsBase64String(credentialsBase64))
            {
                byte[] data = Convert.FromBase64String(credentialsBase64);
                string decodedString = Encoding.UTF8.GetString(data);

                if (!string.IsNullOrEmpty(decodedString))
                    return decodedString.Split(':');
            }

            return new string[] { "", "" };
        }

        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int _);
        }
    }
}
