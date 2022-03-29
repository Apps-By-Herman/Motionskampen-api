using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class RegisterRequest
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        
        [Required] 
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}
