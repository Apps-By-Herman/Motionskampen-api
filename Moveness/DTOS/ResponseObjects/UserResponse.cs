using Moveness.Models;
using System.Collections.Generic;

namespace Moveness.DTOS.ResponseObjects
{
    public class UserResponse
    {
        public string Id { get; set; }
        public string ProfileImageURL { get; set; }
        public string Username { get; set; }

        public static UserResponse From(ApplicationUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                ProfileImageURL = user.ProfileImageURL,
                Username = user.UserName
            };
        }
    }
}
