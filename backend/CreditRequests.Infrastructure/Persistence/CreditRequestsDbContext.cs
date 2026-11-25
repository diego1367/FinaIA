using CreditRequests.Domain;
using Microsoft.EntityFrameworkCore;

namespace CreditRequests.Infrastructure.Persistence;

public sealed class CreditRequestsDbContext(DbContextOptions<CreditRequestsDbContext> options) : DbContext(options)
{
    public DbSet<CreditRequest> CreditRequests => Set<CreditRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var request = modelBuilder.Entity<CreditRequest>();
        request.ToTable("CreditRequests", "dbo");
        request.HasKey(item => item.Id);
        request.Property(item => item.DocumentNumber).HasMaxLength(20).IsRequired();
        request.Property(item => item.FullName).HasMaxLength(100).IsRequired();
        request.Property(item => item.RequestedAmount).HasPrecision(15, 2);
        request.Property(item => item.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        request.Property(item => item.CreatedAtUtc).HasColumnType("datetime2").IsRequired();
        request.Property(item => item.RowVersion).IsRowVersion();
        request.HasIndex(item => new { item.CreatedAtUtc, item.Id }).IsDescending(true, true);
    }
}
