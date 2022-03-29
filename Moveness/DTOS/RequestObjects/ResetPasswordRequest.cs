using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class ResetPasswordRequest
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}
