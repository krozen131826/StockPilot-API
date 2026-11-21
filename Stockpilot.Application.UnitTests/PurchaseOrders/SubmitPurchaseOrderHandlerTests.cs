using FluentAssertions;
using NSubstitute;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Features.PurchaseOrder.Commands;
using Stockpilot.Domain.Entities;
using Stockpilot.Domain.Enums;
using Xunit;

namespace Stockpilot.Application.UnitTests.PurchaseOrders;

public class SubmitPurchaseOrderHandlerTests
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitPurchaseOrderHandlerTests()
    {
        _purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderIsDraft_ShouldSubmitPurchaseOrder()
    {
        // Arrange
        var purchaseOrderId = Guid.NewGuid();
        var purchaseOrder = new PurchaseOrder
        {
            Id = purchaseOrderId,
            Status = PurchaseOrderStatus.Draft,
            Items = new List<PurchaseOrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = purchaseOrderId,
                    ProductId = Guid.NewGuid(),
                    QuantityOrdered = 10
                }
            }
        };

        _purchaseOrderRepository
            .GetPurchaseOrderByIdAsync(purchaseOrderId, Arg.Any<CancellationToken>())
            .Returns(purchaseOrder);

        var request = new SubmitPurchaseOrderCommand(purchaseOrderId);
        var handler = new SubmitPurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _unitOfWork);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        purchaseOrder.Status.Should().Be(PurchaseOrderStatus.Submitted);
        purchaseOrder.SubmittedAt.Should().NotBeNull();

        await _unitOfWork.Received(1).SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var purchaseOrderId = Guid.NewGuid();

        _purchaseOrderRepository
            .GetPurchaseOrderByIdAsync(purchaseOrderId, Arg.Any<CancellationToken>())
            .Returns((PurchaseOrder?)null);

        var request = new SubmitPurchaseOrderCommand(purchaseOrderId);
        var handler = new SubmitPurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.NotFoundExceptions>();

        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderIsNotDraft_ShouldThrowBadRequestException()
    {
        // Arrange
        var purchaseOrderId = Guid.NewGuid();
        var purchaseOrder = new PurchaseOrder
        {
            Id = purchaseOrderId,
            Status = PurchaseOrderStatus.Submitted,
            Items = new List<PurchaseOrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = purchaseOrderId,
                    ProductId = Guid.NewGuid(),
                    QuantityOrdered = 10
                }
            }
        };

        _purchaseOrderRepository
            .GetPurchaseOrderByIdAsync(purchaseOrderId, Arg.Any<CancellationToken>())
            .Returns(purchaseOrder);

        var request = new SubmitPurchaseOrderCommand(purchaseOrderId);
        var handler = new SubmitPurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.BadRequestExceptions>();
        purchaseOrder.Status.Should().Be(PurchaseOrderStatus.Submitted);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderHasNoItems_ShouldThrowBadRequestException()
    {
        // Arrange
        var purchaseOrderId = Guid.NewGuid();
        var purchaseOrder = new PurchaseOrder
        {
            Id = purchaseOrderId,
            Status = PurchaseOrderStatus.Draft,
            Items = new List<PurchaseOrderItem>()
        };

        _purchaseOrderRepository
            .GetPurchaseOrderByIdAsync(purchaseOrderId, Arg.Any<CancellationToken>())
            .Returns(purchaseOrder);

        var request = new SubmitPurchaseOrderCommand(purchaseOrderId);
        var handler = new SubmitPurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.BadRequestExceptions>();
        purchaseOrder.Status.Should().Be(PurchaseOrderStatus.Draft);

        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }
}
