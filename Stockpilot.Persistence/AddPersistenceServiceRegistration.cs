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
        services.AddScoped<ISuppllierRepository, SupplierRepository>();

        services.AddDbContext<StockpilotDbContext>(options =>
           options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;

    }
}