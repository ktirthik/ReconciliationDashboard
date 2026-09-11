using Microsoft.EntityFrameworkCore;
using ReconciliationDashboard.Api.Models;

namespace ReconciliationDashboard.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.AccountNumber).HasMaxLength(50).IsRequired();
            e.Property(a => a.CustomerName).HasMaxLength(200).IsRequired();
            e.Property(a => a.FlagReason).HasMaxLength(500);
            // Store the enum as a string so migrations are readable and adding a new
            // status value doesn't break existing rows.
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
