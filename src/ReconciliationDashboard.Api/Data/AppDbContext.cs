using Microsoft.EntityFrameworkCore;
using ReconciliationDashboard.Api.Models;

namespace ReconciliationDashboard.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CaseNote> CaseNotes => Set<CaseNote>();
    public DbSet<CorrectionRecord> Corrections => Set<CorrectionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.AccountNumber).HasMaxLength(50).IsRequired();
            e.Property(a => a.CustomerName).HasMaxLength(200).IsRequired();
            e.Property(a => a.FlagReason).HasMaxLength(500);
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<CaseNote>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Title).HasMaxLength(200).IsRequired();
            e.Property(c => c.Tags).HasMaxLength(500);
        });

        modelBuilder.Entity<CorrectionRecord>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CorrectionType).HasConversion<string>().HasMaxLength(30);
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Description).HasMaxLength(500).IsRequired();
            e.Property(c => c.OriginalValue).HasMaxLength(500);
            e.Property(c => c.CorrectedValue).HasMaxLength(500);
            e.HasOne(c => c.Account).WithMany().HasForeignKey(c => c.AccountId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
