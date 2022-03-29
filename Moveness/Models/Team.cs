using System.Collections.Generic;

namespace Moveness.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }

        //Relations
        public List<ApplicationUserTeam> Users { get; set; }
        public List<TeamChallenge> Challenges { get; set; }
        public ApplicationUser Owner { get; set; }
    }
}