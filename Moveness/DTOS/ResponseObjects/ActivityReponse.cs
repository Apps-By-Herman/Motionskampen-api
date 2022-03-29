using Moveness.Models;
using System;

namespace Moveness.DTOS.ResponseObjects
{
    public class ActivityResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ActiveMinutes { get; set; }
        public DateTime Created { get; set; }
        public string Username { get; set; }

        public static ActivityResponse From(Activity activity)
        {
            return new ActivityResponse
            {
                Id = activity.Id,
                Created = activity.Created,
                ActiveMinutes = activity.ActiveMinutes,
                Name = activity.Name,
                Username = activity.User.UserName
            };
        }
    }
}
