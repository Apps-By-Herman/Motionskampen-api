using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PutUserRequest
    {
        [Required]
        public string Id { get; set; }
        
        public string ProfileImageURL { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string DeviceToken { get; set; }
    }
}
