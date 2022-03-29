using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PostSearchRequest
    {
        [Required]
        public string Query { get; set; }
    }
}
