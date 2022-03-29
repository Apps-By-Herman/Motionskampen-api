namespace Moveness.Models
{
    public class ApplicationUserChallenge
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ChallengeId { get; set; }
        public Challenge Challenge { get; set; }
    }
}
