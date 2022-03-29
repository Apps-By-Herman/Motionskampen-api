using Moveness.Models;

namespace Moveness.DTOS.ResponseObjects
{
    public class MeResponse
    {
        public string Id { get; set; }
        public string ProfileImageURL { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }

        public static MeResponse From(ApplicationUser user)
        {
            return new MeResponse
            {
                Id = user.Id,
                ProfileImageURL = user.ProfileImageURL,
                Username = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed
            };
        }
    }
}
