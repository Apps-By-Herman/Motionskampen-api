using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PostTeamRequest
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Description { get; set; }

        public List<string> UserIds { get; set; }
    }
}
