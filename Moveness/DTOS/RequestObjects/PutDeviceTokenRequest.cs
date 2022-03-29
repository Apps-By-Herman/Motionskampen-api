using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PutDeviceTokenRequest
    {
        [Required]
        public string DeviceToken { get; set; }
    }
}
