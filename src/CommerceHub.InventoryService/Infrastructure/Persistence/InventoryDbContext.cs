using CommerceHub.InventoryService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CommerceHub.InventoryService.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<SkuStock> SkuStocks => Set<SkuStock>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationItem> ReservationItems => Set<ReservationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SkuStock>(entity =>
        {
            entity.ToTable("sku_stocks");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.Sku).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.Property(e => e.QuantityOnHand);
            entity.Property(e => e.ReservedQuantity);
            entity.Property(e => e.RowVersion).IsRowVersion();
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("reservations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderId);
            entity.Property(e => e.CustomerId);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.ExpiresAt);
            entity.HasIndex(e => e.OrderId).IsUnique();
            entity.HasMany(e => e.Items)
                .WithOne(i => i.Reservation)
                .HasForeignKey(i => i.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReservationItem>(entity =>
        {
            entity.ToTable("reservation_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReservationId);
            entity.Property(e => e.ProductId);
            entity.Property(e => e.Sku).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Quantity);
        });

        base.OnModelCreating(modelBuilder);
    }
}
