using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PostReplyChallengeRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public bool Accepted { get; set; }
    }
}
