using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Stockpilot.Api.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "StockPilot-Integration-Test-Secret-Key-Sufficiently-Long-1234567890",
                ["JwtSettings:Issuer"] = "StockPilot.IntegrationTests",
                ["JwtSettings:Audience"] = "StockPilot.IntegrationTests",
                ["JwtSettings:ExpirationInDays"] = "7"
            });
        });
    }
}
