using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stockpilot.Application.Contracts;
using Stockpilot.Persistence.Repository;

namespace Stockpilot.Persistence;

public static class AddPersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IPurchaseOrderItemRepository, PurchaseOrderItemRepository>();

        services.AddDbContext<StockpilotDbContext>(options =>
           options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;

    }
}