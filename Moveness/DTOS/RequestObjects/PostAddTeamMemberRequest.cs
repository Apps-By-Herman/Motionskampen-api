using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PostAddTeamMemberRequest
    {
        [Required]
        public int TeamId { get; set; }

        [Required]
        public List<string> UserIds { get; set; }
    }
}
