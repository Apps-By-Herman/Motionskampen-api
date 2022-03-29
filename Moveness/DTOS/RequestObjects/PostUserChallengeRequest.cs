using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PostUserChallengeRequest
    {
        [Required]
        public string ChallengedId { get; set; }
    }
}
