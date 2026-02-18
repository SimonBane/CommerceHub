using CommerceHub.PaymentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.PaymentService.Infrastructure.Persistence;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId);
            entity.Property(e => e.CustomerId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.CurrencyCode).HasMaxLength(3);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.ExternalTransactionId).HasMaxLength(100);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.AuthorizedAt);
            entity.HasIndex(e => e.OrderId).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
