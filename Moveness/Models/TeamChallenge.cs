namespace Moveness.Models
{
    public class TeamChallenge
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public int ChallengeId { get; set; }
        public Challenge Challenge { get; set; }
    }
}
