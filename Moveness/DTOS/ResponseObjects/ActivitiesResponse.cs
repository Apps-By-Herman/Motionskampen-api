using Moveness.Models;
using System.Collections.Generic;

namespace Moveness.DTOS.ResponseObjects
{
    public class ActivitiesResponse
    {
        public List<ActivityResponse> Activities { get; set; }
        
        public static ActivitiesResponse From(List<Activity> activities)
        {
            var activitesReponse = new List<ActivityResponse>();

            foreach (var activity in activities)
            {
                activitesReponse.Add(new ActivityResponse
                {
                    ActiveMinutes = activity.ActiveMinutes,
                    Created = activity.Created,
                    Id = activity.Id,
                    Name = activity.Name,
                    Username = activity.User.UserName
                });
            }

            return new ActivitiesResponse { Activities = activitesReponse };
        }
    }
}
