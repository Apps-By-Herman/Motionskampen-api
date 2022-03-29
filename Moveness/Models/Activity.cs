using System;
using System.Collections.Generic;

namespace Moveness.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ActiveMinutes { get; set; }
        public DateTime Created { get; set; }

        //Relations
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<ActivitityChallange> Challenges { get; set; }
    }
}
