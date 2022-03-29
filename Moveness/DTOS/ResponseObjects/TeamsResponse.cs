using Moveness.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Moveness.DTOS.ResponseObjects
{
    public class TeamsResponse
    {
        public List<TeamResponse> Teams { get; set; }

        public static TeamsResponse From(List<ApplicationUserTeam> teams)
        {
            var teamsResponse = new List<TeamResponse>();

            foreach (var applicationUserTeam in teams)
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

                teamsResponse.Add(new TeamResponse
                {
                    Id = applicationUserTeam.TeamId,
                    Description = applicationUserTeam.Team.Description,
                    ImageURL = applicationUserTeam.Team.ImageURL,
                    Name = applicationUserTeam.Team.Name,
                    Members = memebers,
                    ActiveChallenges = challengesResponse,
                    FinishedChallenges = finishedChallengesResponse,
                    IsOwner = applicationUserTeam.Team.Owner == applicationUserTeam.User
                });
            }

            return new TeamsResponse { Teams = teamsResponse };
        }

        public static TeamsResponse From(List<Team> teams, ApplicationUser user)
        {
            var teamsResponse = new List<TeamResponse>();

            foreach (var team in teams)
            {
                var memebers = new List<UserResponse>();
                foreach (var u in team.Users)
                {
                    memebers.Add(new UserResponse
                    {
                        Id = u.User.Id,
                        ProfileImageURL = u.User.ProfileImageURL,
                        Username = u.User.UserName,
                    });
                }

                teamsResponse.Add(new TeamResponse
                {
                    Id = team.Id,
                    Description = team.Description,
                    ImageURL = team.ImageURL,
                    Name = team.Name,
                    Members = memebers,
                    IsOwner = team.Owner == user
                });
            }

            return new TeamsResponse { Teams = teamsResponse };
        }
    }
}
