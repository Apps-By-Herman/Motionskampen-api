using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Moveness.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string ProfileImageURL { get; set; }
        public string DeviceToken { get; set; }
        public string PreferredLanguage { get; set; }

        //Relations
        public List<Activity> Activities { get; set; }
        public List<ApplicationUserTeam> Teams { get; set; }
        public List<ApplicationUserChallenge> Challenges { get; set; }
    }
}
