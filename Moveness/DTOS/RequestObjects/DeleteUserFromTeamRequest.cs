using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class DeleteUserFromTeamRequest
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public int TeamId { get; set; }
    }
}
