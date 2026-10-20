using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Stockpilot.Domain.Common;
using Stockpilot.Domain.Entities;
using System.Security.Claims;

namespace Stockpilot.Persistence;

public class StockpilotDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public StockpilotDbContext(
        DbContextOptions<StockpilotDbContext> options,
        IHttpContextAccessor? httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Category> Categories
    => Set<Category>();
    public DbSet<Product> Products
    => Set<Product>();
    public DbSet<PurchaseOrder> PurchaseOrders
    => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems
    => Set<PurchaseOrderItem>();
    public DbSet<StockMovement> StockMovements
    => Set<StockMovement>();
    public DbSet<Supplier> Suppliers
    => Set<Supplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(StockpilotDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {

        var now = DateTime.UtcNow;
        var userIdString = _httpContextAccessor?.HttpContext?.User?.FindFirst("Id")?.Value
                           ?? _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? _httpContextAccessor?.HttpContext?.User?.Identity?.Name;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
                entry.Entity.LastModifiedDate = now;
                entry.Entity.CreatedBy ??= userIdString;
                entry.Entity.LastModifiedBy = userIdString;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedDate = now;
                entry.Entity.LastModifiedBy = userIdString;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
