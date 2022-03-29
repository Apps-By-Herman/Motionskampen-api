using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PostActivityRequest
    {
        public string Name { get; set; }
        
        [Required]
        public int ActiveMinutes { get; set; }
    }
}
