using JV.ResultUtilities.Demo.Orders.Models;
using JV.ResultUtilities.Demo.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace JV.ResultUtilities.Demo.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Sku).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.Category).HasConversion<string>();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(200);
            entity.Property(o => o.CustomerPhone).HasMaxLength(30);
            entity.Property(o => o.Status).HasConversion<string>();
            entity.Property(o => o.TrackingUri).HasConversion(
                v => v != null ? v.ToString() : null,
                v => v != null ? new Uri(v) : null);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Ignore(o => o.TotalAmount);
            entity.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.OrderId);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(l => l.UnitPrice).HasPrecision(18, 2);
            entity.Ignore(l => l.LineTotal);
            entity.HasIndex(l => new { l.OrderId, l.ProductId });
            entity.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId);
        });
    }
}
