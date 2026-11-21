using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stockpilot.Application.Features.PurchaseOrder.Dtos;
using Stockpilot.Application.Features.PurchaseOrderItem.Dto;
using Stockpilot.Domain.Enums;
using Xunit;

namespace Stockpilot.Api.IntegrationTests;

public class PurchaseOrderControllerTests
{
    [Fact]
    public async Task AdministratorCanAddPurchaseOrder()
    {
        using var factory = new CustomWebApplicationFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);
        using var client = CreateAuthenticatedClient(factory, mediator, "Administrator");

        var response = await client.PostAsJsonAsync("/api/purchaseOrder/add", CreatePurchaseOrderRequest(), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.PurchaseOrder.Commands.AddPurchaseOrderCommand));
    }

    [Fact]
    public async Task StaffCanAddPurchaseOrder()
    {
        using var factory = new CustomWebApplicationFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);
        using var client = CreateAuthenticatedClient(factory, mediator, "Staff");

        var response = await client.PostAsJsonAsync("/api/purchaseOrder/add", CreatePurchaseOrderRequest(), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.PurchaseOrder.Commands.AddPurchaseOrderCommand));
    }

    [Fact]
    public async Task AdministratorCanListPurchaseOrders()
    {
        using var factory = new CustomWebApplicationFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);
        using var client = CreateAuthenticatedClient(factory, mediator, "Administrator");

        var response = await client.GetAsync("/api/purchaseOrder/list", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.PurchaseOrder.Queries.GetPurchaseOrderListQuery));
    }

    [Fact]
    public async Task StaffCanListPurchaseOrders()
    {
        using var factory = new CustomWebApplicationFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);
        using var client = CreateAuthenticatedClient(factory, mediator, "Staff");

        var response = await client.GetAsync("/api/purchaseOrder/list", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.PurchaseOrder.Queries.GetPurchaseOrderListQuery));
    }

    [Fact]
    public async Task AdministratorCanGetPurchaseOrderById()
    {
        using var factory = new CustomWebApplicationFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);
        using var client = CreateAuthenticatedClient(factory, mediator, "Administrator");

        var response = await client.GetAsync($"/api/purchaseOrder/getById/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.PurchaseOrder.Queries.GetPurchaseOrderByIdQuery));
    }

    [Fact]
    public async Task StaffCanGetPurchaseOrderById()
    {
        using var factory = new CustomWebApplicationFactory();
        var mediator = TestMediatorDouble.Create(out var mediatorDouble);
        using var client = CreateAuthenticatedClient(factory, mediator, "Staff");

        var response = await client.GetAsync($"/api/purchaseOrder/getById/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        mediatorDouble.ReceivedRequestTypes.Should().Contain(typeof(Stockpilot.Application.Features.PurchaseOrder.Queries.GetPurchaseOrderByIdQuery));
    }

    [Fact]
    public async Task UnauthenticatedAddPurchaseOrder_ShouldReturnUnauthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/purchaseOrder/add", CreatePurchaseOrderRequest(), TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnauthenticatedListPurchaseOrders_ShouldReturnUnauthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/purchaseOrder/list", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnauthenticatedGetPurchaseOrderById_ShouldReturnUnauthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/purchaseOrder/getById/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static HttpClient CreateAuthenticatedClient(
        CustomWebApplicationFactory factory,
        IMediator mediator,
        string role)
    {
        var client = factory.WithWebHostBuilder(builder =>
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

        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }

    private static AddPurchaseOrderDto CreatePurchaseOrderRequest() => new()
    {
        SupplierId = Guid.NewGuid(),
        Supplier = "Integration Test Supplier",
        OrderNumber = $"PO-{Guid.NewGuid():N}",
        Status = PurchaseOrderStatus.Draft,
        Items = new List<AddPurchaseOrderItemDto>
        {
            new()
            {
                ProductId = Guid.NewGuid(),
                Product = "Integration Test Product",
                QuantityOrdered = 10,
                QuantityReceived = 0,
                UnitCost = 12.50m
            }
        }
    };
}
