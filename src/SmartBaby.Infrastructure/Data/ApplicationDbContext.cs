using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartBaby.Core.Entities;

namespace SmartBaby.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Baby> Babies { get; set; }
    public DbSet<SleepPeriod> SleepPeriods { get; set; }
    public DbSet<Feeding> Feedings { get; set; }
    public DbSet<Crying> Cryings { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<DailyRoutine> DailyRoutines { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Baby>()
            .HasOne(b => b.User)
            .WithMany(u => u.Babies)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SleepPeriod>()
            .HasOne(s => s.Baby)
            .WithMany(b => b.SleepPeriods)
            .HasForeignKey(s => s.BabyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Feeding>()
            .HasOne(f => f.Baby)
            .WithMany(b => b.Feedings)
            .HasForeignKey(f => f.BabyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Crying>()
            .HasOne(c => c.Baby)
            .WithMany(b => b.Cryings)
            .HasForeignKey(c => c.BabyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Note>()
            .HasOne(n => n.Baby)
            .WithMany(b => b.Notes)
            .HasForeignKey(n => n.BabyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 