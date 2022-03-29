namespace Moveness.Models
{
    public class ActivitityChallange
    {
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }

        public int ChallengeId { get; set; }
        public Challenge Challenge { get; set; }
    }
}
