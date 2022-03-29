using System;
using System.ComponentModel.DataAnnotations;

namespace Moveness.DTOS.RequestObjects
{
    public class PutChallengeRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
     
        [Required]
        public string Message { get; set; }
    }
}
