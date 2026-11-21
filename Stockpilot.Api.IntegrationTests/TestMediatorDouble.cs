using MediatR;

namespace Stockpilot.Api.IntegrationTests;

public sealed class TestMediatorDouble : IMediator
{
    public List<Type> ReceivedRequestTypes { get; } = [];

    public bool AddProductCommandInvoked { get; private set; }

    public static IMediator Create(out TestMediatorDouble mediatorDouble)
    {
        mediatorDouble = new TestMediatorDouble();
        return mediatorDouble;
    }

    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        RecordRequest(request);

        if (request is Stockpilot.Application.Features.Product.Commands.AddProductCommand)
        {
            return Task.FromResult((TResponse)(object)Guid.Empty);
        }

        if (request is Stockpilot.Application.Features.PurchaseOrder.Queries.GetPurchaseOrderListQuery)
        {
            return Task.FromResult((TResponse)(object)new List<Stockpilot.Application.Features.PurchaseOrder.Dtos.GetPurchaseOrderDto>());
        }

        if (request is Stockpilot.Application.Features.PurchaseOrder.Queries.GetPurchaseOrderByIdQuery getPurchaseOrderByIdQuery)
        {
            return Task.FromResult((TResponse)(object)new Stockpilot.Application.Features.PurchaseOrder.Dtos.GetPurchaseOrderByIdDto
            {
                Id = getPurchaseOrderByIdQuery.purchaseOrderId,
                Supplier = string.Empty,
                Items = []
            });
        }

        return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        RecordRequest(request);
        return Task.CompletedTask;
    }

    public Task<object?> Send(
        object request,
        CancellationToken cancellationToken = default)
    {
        RecordRequest(request);
        return Task.FromResult<object?>(request is Stockpilot.Application.Features.Product.Commands.AddProductCommand
            ? Guid.Empty
            : null);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default) =>
        Empty<TResponse>();

    public IAsyncEnumerable<object?> CreateStream(
        object request,
        CancellationToken cancellationToken = default) =>
        Empty<object?>();

    public Task Publish(
        object notification,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification =>
        Task.CompletedTask;

    private void RecordRequest(object request)
    {
        ReceivedRequestTypes.Add(request.GetType());
        AddProductCommandInvoked |= request is Stockpilot.Application.Features.Product.Commands.AddProductCommand;
    }

    private static async IAsyncEnumerable<T> Empty<T>()
    {
        yield break;
    }
}