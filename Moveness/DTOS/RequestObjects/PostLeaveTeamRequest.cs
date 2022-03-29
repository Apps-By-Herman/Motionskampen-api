using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PostLeaveTeamRequest
    {
        [Required]
        public int TeamId { get; set; }
    }
}
