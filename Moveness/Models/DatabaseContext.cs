using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Moveness.Models
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationUserTeam> ApplicationUserTeam { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Many-to-many for ApplicationUser and Team
            modelBuilder.Entity<ApplicationUserTeam>()
                .HasKey(t => new { t.UserId, t.TeamId });

            modelBuilder.Entity<ApplicationUserTeam>()
                .HasOne(pt => pt.User)
                .WithMany(t => t.Teams)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<ApplicationUserTeam>()
                .HasOne(pt => pt.Team)
                .WithMany(p => p.Users)
                .HasForeignKey(pt => pt.TeamId);

            //Many-to-many for ApplicationUser and Challenge
            modelBuilder.Entity<ApplicationUserChallenge>()
                .HasKey(t => new { t.UserId, t.ChallengeId });

            modelBuilder.Entity<ApplicationUserChallenge>()
                .HasOne(pt => pt.User)
                .WithMany(p => p.Challenges)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<ApplicationUserChallenge>()
                .HasOne(pt => pt.Challenge)
                .WithMany(t => t.Users)
                .HasForeignKey(pt => pt.ChallengeId);

            //Many-to-many for Activities and Challenge
            modelBuilder.Entity<ActivitityChallange>()
                .HasKey(t => new { t.ActivityId, t.ChallengeId });

            modelBuilder.Entity<ActivitityChallange>()
                .HasOne(pt => pt.Activity)
                .WithMany(p => p.Challenges)
                .HasForeignKey(pt => pt.ActivityId);

            modelBuilder.Entity<ActivitityChallange>()
                .HasOne(pt => pt.Challenge)
                .WithMany(t => t.Activities)
                .HasForeignKey(pt => pt.ChallengeId);

            //Many-to-many for Team and Challenge
            modelBuilder.Entity<TeamChallenge>()
                .HasKey(t => new { t.TeamId, t.ChallengeId });

            modelBuilder.Entity<TeamChallenge>()
                .HasOne(pt => pt.Team)
                .WithMany(p => p.Challenges)
                .HasForeignKey(pt => pt.TeamId);

            modelBuilder.Entity<TeamChallenge>()
                .HasOne(pt => pt.Challenge)
                .WithMany(t => t.Teams)
                .HasForeignKey(pt => pt.ChallengeId);
        }
    }
}
