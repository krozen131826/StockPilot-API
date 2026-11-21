using FluentAssertions;
using NSubstitute;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Features.PurchaseOrder.Commands;
using Stockpilot.Domain.Entities;
using Stockpilot.Domain.Enums;
using Xunit;

namespace Stockpilot.Application.UnitTests.PurchaseOrders;

public class ReceivePurchaseOrderHandlerTests
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ReceivePurchaseOrderHandlerTests()
    {
        _purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _stockMovementRepository = Substitute.For<IStockMovementRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderIsSubmitted_ShouldReceivePurchaseOrder()
    {
        // Arrange
        var purchaseOrderId = Guid.NewGuid();
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var movements = new List<StockMovement>();
        var firstProduct = new Product
        {
            Id = firstProductId,
            QuantityOnHand = 30
        };
        var secondProduct = new Product
        {
            Id = secondProductId,
            QuantityOnHand = 5
        };
        var purchaseOrder = new PurchaseOrder
        {
            Id = purchaseOrderId,
            OrderNumber = "PO-2001",
            Status = PurchaseOrderStatus.Submitted,
            Items = new List<PurchaseOrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = purchaseOrderId,
                    ProductId = firstProductId,
                    Product = firstProduct,
                    QuantityOrdered = 7
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = purchaseOrderId,
                    ProductId = secondProductId,
                    Product = secondProduct,
                    QuantityOrdered = 12
                }
            }
        };

        _purchaseOrderRepository
            .GetPurchaseOrderByIdAsync(purchaseOrderId, Arg.Any<CancellationToken>())
            .Returns(purchaseOrder);
        _currentUserService.UserId.Returns(userId);
        _currentUserService.FullName.Returns("Receiving User");
        _stockMovementRepository.AddStockMovement(
            Arg.Do<StockMovement>(movement => movements.Add(movement)),
            Arg.Any<CancellationToken>());

        var request = new ReceivePurchaseOrderCommand(purchaseOrderId);
        var handler = new ReceivePurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _productRepository,
            _stockMovementRepository,
            _currentUserService,
            _unitOfWork);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        purchaseOrder.Status.Should().Be(PurchaseOrderStatus.Received);
        purchaseOrder.ReceivedAt.Should().NotBeNull();
        firstProduct.QuantityOnHand.Should().Be(37);
        secondProduct.QuantityOnHand.Should().Be(17);
        purchaseOrder.Items.ElementAt(0).QuantityReceived.Should().Be(7);
        purchaseOrder.Items.ElementAt(1).QuantityReceived.Should().Be(12);

        _stockMovementRepository.Received(2).AddStockMovement(
            Arg.Any<StockMovement>(),
            Arg.Any<CancellationToken>());
        movements.Should().HaveCount(2);
        movements.Should().ContainSingle(movement =>
            movement.ProductId == firstProductId &&
            movement.PurchaseOrderId == purchaseOrderId &&
            movement.QuantityChange == 7 &&
            movement.QuantityBefore == 30 &&
            movement.QuantityAfter == 37 &&
            movement.MovementType == StockMovementType.StockIn &&
            movement.PerformedByUserId == userId &&
            movement.PerformedByUser == "Receiving User");
        movements.Should().ContainSingle(movement =>
            movement.ProductId == secondProductId &&
            movement.PurchaseOrderId == purchaseOrderId &&
            movement.QuantityChange == 12 &&
            movement.QuantityBefore == 5 &&
            movement.QuantityAfter == 17 &&
            movement.MovementType == StockMovementType.StockIn &&
            movement.PerformedByUserId == userId &&
            movement.PerformedByUser == "Receiving User");

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

        var request = new ReceivePurchaseOrderCommand(purchaseOrderId);
        var handler = new ReceivePurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _productRepository,
            _stockMovementRepository,
            _currentUserService,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.NotFoundExceptions>();

        _stockMovementRepository.DidNotReceive().AddStockMovement(
            Arg.Any<StockMovement>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderIsNotSubmitted_ShouldThrowBadRequestException()
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
                    ProductId = Guid.NewGuid(),
                    QuantityOrdered = 10
                }
            }
        };

        _purchaseOrderRepository
            .GetPurchaseOrderByIdAsync(purchaseOrderId, Arg.Any<CancellationToken>())
            .Returns(purchaseOrder);

        var request = new ReceivePurchaseOrderCommand(purchaseOrderId);
        var handler = new ReceivePurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _productRepository,
            _stockMovementRepository,
            _currentUserService,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.BadRequestExceptions>();
        purchaseOrder.Status.Should().Be(PurchaseOrderStatus.Draft);

        _stockMovementRepository.DidNotReceive().AddStockMovement(
            Arg.Any<StockMovement>(),
            Arg.Any<CancellationToken>());
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
            Status = PurchaseOrderStatus.Submitted,
            Items = new List<PurchaseOrderItem>()
        };

        _purchaseOrderRepository
            .GetPurchaseOrderByIdAsync(purchaseOrderId, Arg.Any<CancellationToken>())
            .Returns(purchaseOrder);

        var request = new ReceivePurchaseOrderCommand(purchaseOrderId);
        var handler = new ReceivePurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _productRepository,
            _stockMovementRepository,
            _currentUserService,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.BadRequestExceptions>();
        purchaseOrder.Status.Should().Be(PurchaseOrderStatus.Submitted);

        _stockMovementRepository.DidNotReceive().AddStockMovement(
            Arg.Any<StockMovement>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var purchaseOrderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var purchaseOrder = new PurchaseOrder
        {
            Id = purchaseOrderId,
            Status = PurchaseOrderStatus.Submitted,
            Items = new List<PurchaseOrderItem>
            {
                new()
                {
                    ProductId = productId,
                    Product = null!,
                    QuantityOrdered = 10
                }
            }
        };

        _purchaseOrderRepository
            .GetPurchaseOrderByIdAsync(purchaseOrderId, Arg.Any<CancellationToken>())
            .Returns(purchaseOrder);
        _productRepository
            .GetProductByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var request = new ReceivePurchaseOrderCommand(purchaseOrderId);
        var handler = new ReceivePurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _productRepository,
            _stockMovementRepository,
            _currentUserService,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.NotFoundExceptions>();

        _stockMovementRepository.DidNotReceive().AddStockMovement(
            Arg.Any<StockMovement>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }
}
