using Moveness.Models;
using System.Collections.Generic;

namespace Moveness.DTOS.ResponseObjects
{
    public class UsersResponse
    {
        public List<UserResponse> Users { get; set; }

        public static UsersResponse From(List<ApplicationUser> users)
        {
            var usersResponse = new List<UserResponse>();

            foreach (var user in users)
            {
                usersResponse.Add(new UserResponse
                {
                    Id = user.Id,
                    ProfileImageURL = user.ProfileImageURL,
                    Username = user.UserName
                });
            }

            return new UsersResponse { Users = usersResponse };
        }
    }
}
