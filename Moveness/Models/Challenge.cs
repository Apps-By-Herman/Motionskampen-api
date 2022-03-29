using System;
using System.Collections.Generic;

namespace Moveness.Models
{
    public class Challenge
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

        //Relations
        public List<ApplicationUserChallenge> Users { get; set; }
        public List<TeamChallenge> Teams { get; set; }
        public List<ActivitityChallange> Activities { get; set; }
        public ApplicationUser Owner { get; set; }
    }
}
