using Microsoft.EntityFrameworkCore;
using Moveness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moveness.DTOS.ResponseObjects
{
    public class TeamResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public List<UserResponse> Members { get; set; }
        public List<ChallengeResponse> ActiveChallenges { get; set; }
        public List<ChallengeResponse> FinishedChallenges { get; set; }
        public bool IsOwner { get; set; }

        public static async Task<TeamResponse> From(ApplicationUserTeam applicationUserTeam, DatabaseContext _context)
        {
            var memebers = new List<UserResponse>();
            foreach (var u in applicationUserTeam.Team.Users)
            {
                memebers.Add(new UserResponse
                {
                    Id = u.User.Id,
                    ProfileImageURL = u.User.ProfileImageURL,
                    Username = u.User.UserName,
                });
            }

            var activeOrUnansweredChallenges = applicationUserTeam.Team.Challenges.Where(x => x.Challenge.Accepted != false && 
                                                                                                x.Challenge.EndTime > DateTime.UtcNow)
                                                                                  .ToList();

            var challengesResponse = new List<ChallengeResponse>();
            foreach (var challenge in activeOrUnansweredChallenges)
            {
                var challengingTeam = await _context.Teams.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == challenge.Challenge.ChallengingTeamId);
                var challengedTeam = await _context.Teams.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == challenge.Challenge.ChallengedTeamId);

                var challengingName = challengingTeam.Name;
                var challengingMinutes = challenge.Challenge.Activities.Select(x => x.Activity)
                                                         .Where(x => challengingTeam.Users.Select(x => x.UserId).Contains(x.User.Id))
                                                         .Sum(x => x.ActiveMinutes);

                var challengedName = challengedTeam.Name;
                var challengedMinutes = challenge.Challenge.Activities.Select(x => x.Activity)
                                                         .Where(x => challengedTeam.Users.Select(x => x.UserId).Contains(x.User.Id))
                                                         .Sum(x => x.ActiveMinutes);

                var activities = challenge.Challenge.Activities.OrderByDescending(x => x.ActivityId)
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

                challengesResponse.Add(new ChallengeResponse
                {
                    Id = challenge.Challenge.Id,
                    Message = challenge.Challenge.Message,
                    EndTime = challenge.Challenge.EndTime,
                    Accepted = challenge.Challenge.Accepted,
                    StartTime = challenge.Challenge.StartTime,
                    IsTeamChallenge = challenge.Challenge.IsTeamChallenge,
                    ChallengingTeamId = challenge.Challenge.ChallengingTeamId,
                    ChallengedTeamId = challenge.Challenge.ChallengedTeamId,
                    ChallengingUserId = challenge.Challenge.ChallengingUserId,
                    ChallengedUserId = challenge.Challenge.ChallengedUserId,
                    LatestActivities = latestActivities,
                    ChallengedMinutes = challengedMinutes,
                    ChallengedName = challengedName,
                    ChallengingMinutes = challengingMinutes,
                    ChallengingName = challengingName,
                });
            }

            var finishedChallenges = applicationUserTeam.Team.Challenges.Where(x => x.Challenge.Accepted == true && 
                                                                                    x.Challenge.EndTime < DateTime.UtcNow)
                                                                        .ToList();

            var finishedChallengesResponse = new List<ChallengeResponse>();
            foreach (var challenge in finishedChallenges)
            {
                var challengingName = challenge.Team.Name;
                var challengingMinutes = challenge.Challenge.Activities.Select(x => x.Activity)
                                                         .Where(x => challenge.Team.Users.Select(x => x.UserId).Contains(x.User.Id))
                                                         .Sum(x => x.ActiveMinutes);

                var challengedName = challenge.Team.Name;
                var challengedMinutes = challenge.Challenge.Activities.Select(x => x.Activity)
                                                         .Where(x => challenge.Team.Users.Select(x => x.UserId).Contains(x.User.Id))
                                                         .Sum(x => x.ActiveMinutes);

                var activities = challenge.Challenge.Activities.OrderByDescending(x => x.ActivityId)
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

                finishedChallengesResponse.Add(new ChallengeResponse
                {
                    Id = challenge.Challenge.Id,
                    Message = challenge.Challenge.Message,
                    EndTime = challenge.Challenge.EndTime,
                    Accepted = challenge.Challenge.Accepted,
                    StartTime = challenge.Challenge.StartTime,
                    IsTeamChallenge = challenge.Challenge.IsTeamChallenge,
                    ChallengingTeamId = challenge.Challenge.ChallengingTeamId,
                    ChallengedTeamId = challenge.Challenge.ChallengedTeamId,
                    ChallengingUserId = challenge.Challenge.ChallengingUserId,
                    ChallengedUserId = challenge.Challenge.ChallengedUserId,
                    LatestActivities = latestActivities,
                    ChallengedMinutes = challengedMinutes,
                    ChallengedName = challengedName,
                    ChallengingMinutes = challengingMinutes,
                    ChallengingName = challengingName,
                });
            }

            return new TeamResponse
            {
                Id = applicationUserTeam.TeamId,
                Description = applicationUserTeam.Team.Description,
                ImageURL = applicationUserTeam.Team.ImageURL,
                Name = applicationUserTeam.Team.Name,
                Members = memebers,
                ActiveChallenges = challengesResponse,
                FinishedChallenges = finishedChallengesResponse,
                IsOwner = applicationUserTeam.Team.Owner == applicationUserTeam.User
            };
        }
    }
}
