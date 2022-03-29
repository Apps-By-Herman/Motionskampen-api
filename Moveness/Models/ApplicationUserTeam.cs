namespace Moveness.Models
{
    public class ApplicationUserTeam
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }
    }
}
