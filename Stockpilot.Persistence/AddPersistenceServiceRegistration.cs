
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Stockpilot.Persistence;

public static class AddPersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StockpilotDbContext>(options =>
           options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;

    }
}