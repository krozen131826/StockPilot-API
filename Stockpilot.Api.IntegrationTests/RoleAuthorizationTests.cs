using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    public async Task AdministratorCreatingProduct_ShouldPassAuthorizationAndInvokeMediator()
    {
        using var factory = CreateFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton(mediator);

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

        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

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

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        mediatorDouble.AddProductCommandInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task AdministratorUpdatingProduct_ShouldPassAuthorizationAndInvokeMediator()
    {
        using var factory = CreateFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton(mediator);

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

        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.PutAsJsonAsync($"/api/product/update/{Guid.NewGuid()}", new
        {
            CategoryId = Guid.NewGuid(),
            SKU = "TEST-SKU",
            Name = "Updated Product",
            Description = "Updated integration test product",
            CostPrice = 10m,
            SellingPrice = 15m,
            ReorderLevel = 5,
            IsActive = true
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.Product.Commands.UpdateProductCommand));
    }

    [Fact]
    public async Task AdministratorDeletingProduct_ShouldPassAuthorizationAndInvokeMediator()
    {
        using var factory = CreateFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton(mediator);

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

        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.PutAsync(
            $"/api/product/delete/{Guid.NewGuid()}",
            new StringContent(string.Empty),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.Product.Commands.DeleteProductCommand));
    }

    [Fact]
    public async Task AdministratorCreatingCategory_ShouldPassAuthorizationAndInvokeMediator()
    {
        using var factory = CreateFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton(mediator);

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

        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.PostAsJsonAsync("/api/category/add", new
        {
            Name = "Integration Test Category",
            Description = "Integration test category"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.Category.Commands.AddCategoryCommand));
    }

    [Fact]
    public async Task AdministratorUpdatingCategory_ShouldPassAuthorizationAndInvokeMediator()
    {
        using var factory = CreateFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton(mediator);

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

        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.PutAsJsonAsync($"/api/category/update/{Guid.NewGuid()}", new
        {
            Name = "Updated Integration Test Category",
            Description = "Updated integration test category"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.Category.Commands.UpdateCategoryCommand));
    }

    [Fact]
    public async Task AdministratorDeletingCategory_ShouldPassAuthorizationAndInvokeMediator()
    {
        using var factory = CreateFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton(mediator);

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

        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.PutAsync(
            $"/api/category/delete/{Guid.NewGuid()}",
            new StringContent(string.Empty),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.Category.Commands.DeleteCategoryCommand));
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
    [Fact]
    public async Task StaffUpdatingProduct_ShouldReturnForbidden()
    {
        using var factory = CreateFactory();
        using var client = CreateStaffClient(factory);

        var response = await client.PutAsJsonAsync($"/api/product/update/{Guid.NewGuid()}", new
        {
            CategoryId = Guid.NewGuid(),
            SKU = "TEST-SKU",
            Name = "Updated Product",
            Description = "Updated integration test product",
            CostPrice = 10m,
            SellingPrice = 15m,
            ReorderLevel = 5,
            IsActive = true
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StaffDeletingProduct_ShouldReturnForbidden()
    {
        using var factory = CreateFactory();
        using var client = CreateStaffClient(factory);

        var response = await client.PutAsync(
            $"/api/product/delete/{Guid.NewGuid()}",
            new StringContent(string.Empty),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StaffUpdatingCategory_ShouldReturnForbidden()
    {
        using var factory = CreateFactory();
        using var client = CreateStaffClient(factory);

        var response = await client.PutAsJsonAsync($"/api/category/update/{Guid.NewGuid()}", new
        {
            Name = "Updated Integration Test Category",
            Description = "Updated integration test category"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StaffDeletingCategory_ShouldReturnForbidden()
    {
        using var factory = CreateFactory();
        using var client = CreateStaffClient(factory);

        var response = await client.PutAsync(
            $"/api/category/delete/{Guid.NewGuid()}",
            new StringContent(string.Empty),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdministratorCreatingSupplier_ShouldPassAuthorizationAndInvokeMediator()
    {
        using var factory = CreateFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton(mediator);

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

        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.PostAsJsonAsync("/api/supplier/add", new
        {
            Name = "Integration Test Supplier",
            ContactPerson = "Test Contact",
            Email = "supplier@example.com",
            Phone = "555-0100",
            Address = "Integration Test Address"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.Supplier.Commands.AddSupplierCommand));
    }

    [Fact]
    public async Task AdministratorUpdatingSupplier_ShouldPassAuthorizationAndInvokeMediator()
    {
        using var factory = CreateFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton(mediator);

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

        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.PutAsJsonAsync($"/api/supplier/update/{Guid.NewGuid()}", new
        {
            Name = "Updated Integration Test Supplier",
            ContactPerson = "Updated Contact",
            Email = "updated-supplier@example.com",
            Phone = "555-0101",
            Address = "Updated Integration Test Address"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.Supplier.Commands.UpdateSupplierCommand));
    }

    [Fact]
    public async Task AdministratorDeletingSupplier_ShouldPassAuthorizationAndInvokeMediator()
    {
        using var factory = CreateFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);

        using var client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMediator>();
                services.AddSingleton(mediator);

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

        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.PutAsync(
            $"/api/supplier/delete/{Guid.NewGuid()}",
            new StringContent(string.Empty),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.Supplier.Commands.DeleteSupplierCommand));
    }

    [Fact]
    public async Task StaffCreatingSupplier_ShouldReturnForbidden()
    {
        using var factory = CreateFactory();
        using var client = CreateStaffClient(factory);

        var response = await client.PostAsJsonAsync("/api/supplier/add", new
        {
            Name = "Integration Test Supplier",
            ContactPerson = "Test Contact",
            Email = "supplier@example.com",
            Phone = "555-0100",
            Address = "Integration Test Address"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StaffUpdatingSupplier_ShouldReturnForbidden()
    {
        using var factory = CreateFactory();
        using var client = CreateStaffClient(factory);

        var response = await client.PutAsJsonAsync($"/api/supplier/update/{Guid.NewGuid()}", new
        {
            Name = "Updated Integration Test Supplier",
            ContactPerson = "Updated Contact",
            Email = "updated-supplier@example.com",
            Phone = "555-0101",
            Address = "Updated Integration Test Address"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StaffDeletingSupplier_ShouldReturnForbidden()
    {
        using var factory = CreateFactory();
        using var client = CreateStaffClient(factory);

        var response = await client.PutAsync(
            $"/api/supplier/delete/{Guid.NewGuid()}",
            new StringContent(string.Empty),
            TestContext.Current.CancellationToken);

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
