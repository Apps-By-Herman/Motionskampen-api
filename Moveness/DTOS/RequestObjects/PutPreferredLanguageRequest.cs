using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PutPreferredLanguageRequest
    {
        [Required]
        public string PreferredLanguage { get; set; }
    }
}
