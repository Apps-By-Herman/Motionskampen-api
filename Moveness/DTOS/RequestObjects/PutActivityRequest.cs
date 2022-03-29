using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PutActivityRequest
    {
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }
        
        [Required]
        public int ActiveMinutes { get; set; }
    }
}
