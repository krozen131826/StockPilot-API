using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Stockpilot.Api.IntegrationTests;

public class RoleAuthorizationTests
{
    [Fact]
    public async Task StaffCreatingProduct_ShouldReturnForbidden()
    {
        using var factory = CreateFactory();
        using var client = CreateStaffClient(factory);

        var response = await client.PostAsJsonAsync("/api/product/add", new
        {
            CategoryId = Guid.NewGuid(),
            SKU = "TEST-SKU",
            Name = "Test Product",
            Description = "Integration test product",
            CostPrice = 10m,
            SellingPrice = 15m,
            ReorderLevel = 5,
            IsActive = true
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StaffCreatingCategory_ShouldReturnForbidden()
    {
        using var factory = CreateFactory();
        using var client = CreateStaffClient(factory);

        var response = await client.PostAsJsonAsync("/api/category/add", new
        {
            Name = "Integration Test Category",
            Description = "Integration test category"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static CustomWebApplicationFactory CreateFactory() => new();

    private static HttpClient CreateStaffClient(CustomWebApplicationFactory factory)
    {
        var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        client.DefaultRequestHeaders.Add("X-Test-Role", "Staff");
        return client;
    }
}
