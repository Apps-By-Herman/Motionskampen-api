using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PostRandomTeamChallengeRequest
    {
        [Required]
        public int ChallengingTeamId { get; set; }
    }
}
