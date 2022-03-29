using Microsoft.EntityFrameworkCore;
using Moveness.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Moveness.DTOS.ResponseObjects
{
    public class ChallengesResponse
    {
        public List<ChallengeResponse> Challenges { get; set; }

        public static async Task<ChallengesResponse> From(List<ApplicationUserChallenge> challenges, DatabaseContext context)
        {
            var challengesReponse = new List<ChallengeResponse>();

            foreach (var applicationUserChallenge in challenges)
            {
                var challenge = applicationUserChallenge.Challenge;

                if(challenge.IsTeamChallenge)
                {
                    var challeningTeam = await context.Teams.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == challenge.ChallengingTeamId);
                    var challengedTeam = await context.Teams.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == challenge.ChallengedTeamId);
                    challengesReponse.Add(ChallengeResponse.From(challenge, challeningTeam, challengedTeam));
                }
                else
                {
                    var challengingUser = await context.Users.FindAsync(challenge.ChallengingUserId);
                    var challengedUser = await context.Users.FindAsync(challenge.ChallengedUserId);
                    challengesReponse.Add(ChallengeResponse.From(challenge, challengingUser, challengedUser));
                }
            }

            return new ChallengesResponse { Challenges = challengesReponse };
        }
    }
}
