using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Moveness.DTOS.ResponseObjects;
using Microsoft.IdentityModel.Tokens;

namespace Moveness.Services
{
    public interface ITokenService
    {
        TokenResponse CreateAccessToken(string userId, string userName);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenResponse CreateAccessToken(string userId, string userName)
        {
            var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Token:AccessExpireMinutes"]));

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.NameIdentifier, userId)
                }),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Token:Key"])), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var t = tokenHandler.WriteToken(token);

            return CreateTokenResource(t, UnixTimeNow(expiry));
        }

        private static TokenResponse CreateTokenResource(string token, long expiry)
            => new TokenResponse { Token = token, Expiry = expiry };

        private long UnixTimeNow(DateTime expiry)
        {
            return (long)Math.Abs((DateTime.UtcNow - expiry).TotalSeconds);
        }
    }
}
