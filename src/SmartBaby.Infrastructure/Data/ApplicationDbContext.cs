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
    
    // Baby Analysis entities
    public DbSet<BabyAnalysis> BabyAnalyses { get; set; }
    public DbSet<BatchAnalysis> BatchAnalyses { get; set; }
    public DbSet<RealtimeAnalysisSession> RealtimeAnalysisSessions { get; set; }
    public DbSet<RealtimeAnalysisUpdate> RealtimeAnalysisUpdates { get; set; }
    public DbSet<AnalysisTag> AnalysisTags { get; set; }
    public DbSet<AnalysisAlert> AnalysisAlerts { get; set; }

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

        // Baby Analysis relationships
        builder.Entity<BabyAnalysis>()
            .HasOne(ba => ba.Baby)
            .WithMany()
            .HasForeignKey(ba => ba.BabyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BatchAnalysis>()
            .HasOne(ba => ba.Baby)
            .WithMany()
            .HasForeignKey(ba => ba.BabyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RealtimeAnalysisSession>()
            .HasOne(ras => ras.Baby)
            .WithMany()
            .HasForeignKey(ras => ras.BabyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RealtimeAnalysisUpdate>()
            .HasOne(rau => rau.Session)
            .WithMany(ras => ras.Updates)
            .HasForeignKey(rau => rau.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AnalysisTag>()
            .HasOne(at => at.Analysis)
            .WithMany(ba => ba.Tags)
            .HasForeignKey(at => at.AnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AnalysisAlert>()
            .HasOne(aa => aa.Baby)
            .WithMany()
            .HasForeignKey(aa => aa.BabyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AnalysisAlert>()
            .HasOne(aa => aa.Analysis)
            .WithMany()
            .HasForeignKey(aa => aa.AnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for better performance
        builder.Entity<BabyAnalysis>()
            .HasIndex(ba => new { ba.BabyId, ba.CreatedAt })
            .HasDatabaseName("IX_BabyAnalysis_BabyId_CreatedAt");

        builder.Entity<BabyAnalysis>()
            .HasIndex(ba => ba.AnalysisType)
            .HasDatabaseName("IX_BabyAnalysis_AnalysisType");

        builder.Entity<RealtimeAnalysisSession>()
            .HasIndex(ras => ras.SessionId)
            .IsUnique()
            .HasDatabaseName("IX_RealtimeAnalysisSession_SessionId");

        builder.Entity<AnalysisAlert>()
            .HasIndex(aa => new { aa.BabyId, aa.CreatedAt })
            .HasDatabaseName("IX_AnalysisAlert_BabyId_CreatedAt");
    }
} 