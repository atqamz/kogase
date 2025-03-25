using Microsoft.EntityFrameworkCore;
using Kogase.Engine.Models;

namespace Kogase.Engine.Data
{
    public class KogaseDbContext : DbContext
    {
        public KogaseDbContext(DbContextOptions<KogaseDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; } = null!;
        public DbSet<Device> Devices { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Metric> Metrics { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Create indexes for performance
            modelBuilder.Entity<Device>()
                .HasIndex(d => new { d.ProjectId, d.DeviceId })
                .IsUnique();
                
            modelBuilder.Entity<Event>()
                .HasIndex(e => new { e.ProjectId, e.EventType, e.Timestamp });
                
            modelBuilder.Entity<Metric>()
                .HasIndex(m => new { m.ProjectId, m.MetricType, m.Period, m.PeriodStart });
                
            modelBuilder.Entity<ProjectUser>()
                .HasIndex(pu => new { pu.ProjectId, pu.UserId })
                .IsUnique();
        }
    }
} 