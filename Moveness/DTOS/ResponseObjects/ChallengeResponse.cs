using Moveness.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Moveness.DTOS.ResponseObjects
{
    public class ChallengeResponse
    {
        public int Id { get; set; }
        public bool? Accepted { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Message { get; set; }
        public string ChallengingUserId { get; set; }
        public string ChallengedUserId { get; set; }
        public int ChallengingTeamId { get; set; }
        public int ChallengedTeamId { get; set; }
        public bool IsTeamChallenge { get; set; }
        public string ChallengingName { get; set; }
        public string ChallengedName { get; set; }
        public int ChallengingMinutes { get; set; }
        public int ChallengedMinutes { get; set; }
        public List<ActivityResponse> LatestActivities { get; set; }

        public static ChallengeResponse From(Challenge challenge, ApplicationUser challengingUser, ApplicationUser challengedUser)
        {
            var challengingName = challengingUser.UserName;
            var challengingMinutes = challenge.Activities.Select(x => x.Activity)
                                                     .Where(x => x.UserId == challengingUser.Id)
                                                     .Sum(x => x.ActiveMinutes);

            var challengedName = challengedUser.UserName;
            var challengedMinutes = challenge.Activities.Select(x => x.Activity)
                                     .Where(x => x.UserId == challengedUser.Id)
                                     .Sum(x => x.ActiveMinutes);

            var activities = challenge.Activities.OrderByDescending(x => x.ActivityId)
                                                 .Select(x => x.Activity)
                                                 .Take(10)
                                                 .OrderByDescending(x => x.Id)
                                                 .ToList();

            var latestActivities = new List<ActivityResponse>();
            foreach (var activity in activities)
            {
                latestActivities.Add(new ActivityResponse
                {
                    Id = activity.Id,
                    Created = activity.Created,
                    ActiveMinutes = activity.ActiveMinutes,
                    Name = activity.Name,
                    Username = activity.User.UserName
                });
            }

            return new ChallengeResponse
            {
                Id = challenge.Id,
                Message = challenge.Message,
                EndTime = challenge.EndTime,
                Accepted = challenge.Accepted,
                StartTime = challenge.StartTime,
                IsTeamChallenge = challenge.IsTeamChallenge,
                ChallengingTeamId = challenge.ChallengingTeamId,
                ChallengedTeamId = challenge.ChallengedTeamId,
                ChallengingUserId = challenge.ChallengingUserId,
                ChallengedUserId = challenge.ChallengedUserId,
                ChallengedMinutes = challengedMinutes,
                ChallengedName = challengedName,
                ChallengingMinutes = challengingMinutes,
                ChallengingName = challengingName,
                LatestActivities = latestActivities,
            };
        }

        public static ChallengeResponse From(Challenge challenge, Team challengingTeam, Team challengedTeam)
        {
            var challengingName = challengingTeam.Name;
            var challengingMinutes = challenge.Activities.Select(x => x.Activity)
                                                     .Where(x => challengingTeam.Users.Select(x => x.UserId).Contains(x.User.Id))
                                                     .Sum(x => x.ActiveMinutes);

            var challengedName = challengedTeam.Name;
            var challengedMinutes = challenge.Activities.Select(x => x.Activity)
                                                     .Where(x => challengedTeam.Users.Select(x => x.UserId).Contains(x.User.Id))
                                                     .Sum(x => x.ActiveMinutes);

            var activities = challenge.Activities.OrderByDescending(x => x.ActivityId)
                                                 .Select(x => x.Activity)
                                                 .Take(10)
                                                 .OrderByDescending(x => x.Id)
                                                 .ToList();

            var latestActivities = new List<ActivityResponse>();
            foreach (var activity in activities)
            {
                latestActivities.Add(new ActivityResponse
                {
                    Id = activity.Id,
                    Created = activity.Created,
                    ActiveMinutes = activity.ActiveMinutes,
                    Name = activity.Name,
                    Username = activity.User.UserName
                });
            }

            return new ChallengeResponse
            {
                Id = challenge.Id,
                Message = challenge.Message,
                EndTime = challenge.EndTime,
                Accepted = challenge.Accepted,
                StartTime = challenge.StartTime,
                IsTeamChallenge = challenge.IsTeamChallenge,
                ChallengingTeamId = challenge.ChallengingTeamId,
                ChallengedTeamId = challenge.ChallengedTeamId,
                ChallengingUserId = challenge.ChallengingUserId,
                ChallengedUserId = challenge.ChallengedUserId,
                ChallengedMinutes = challengedMinutes,
                ChallengedName = challengedName,
                ChallengingMinutes = challengingMinutes,
                ChallengingName = challengingName,
                LatestActivities = latestActivities,
            };
        }
    }
}
