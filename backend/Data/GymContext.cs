using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class GymContext : DbContext
    {
        public GymContext(DbContextOptions<GymContext> options) : base(options) { }

        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<TrainingPlan> TrainingPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Training>()
                .HasOne(t => t.Trainer)
                .WithMany(tr => tr.Trainings)
                .HasForeignKey(t => t.TrainerId);

            modelBuilder.Entity<TrainingPlan>()
                .HasOne(tp => tp.Member)
                .WithMany(m => m.TrainingPlans)
                .HasForeignKey(tp => tp.MemberId);

            modelBuilder.Entity<TrainingPlan>()
                .HasOne(tp => tp.Training)
                .WithMany(t => t.TrainingPlans)
                .HasForeignKey(tp => tp.TrainingId);
        }
    }
}