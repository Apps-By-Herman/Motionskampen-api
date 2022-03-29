using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PostTeamChallengeRequest
    {
        [Required]
        public int ChallengedTeamId { get; set; }

        [Required]
        public int ChallengingTeamId { get; set; }
    }
}
