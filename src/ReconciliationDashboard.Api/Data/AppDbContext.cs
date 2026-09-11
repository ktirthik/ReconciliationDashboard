using Microsoft.EntityFrameworkCore;
using ReconciliationDashboard.Api.Models;

namespace ReconciliationDashboard.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CaseNote> CaseNotes => Set<CaseNote>();

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
    }
}
