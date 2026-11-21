using FluentAssertions;
using NSubstitute;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Features.PurchaseOrder.Commands;
using Stockpilot.Application.Features.PurchaseOrder.Dtos;
using Stockpilot.Application.Features.PurchaseOrderItem.Dto;
using Stockpilot.Domain.Entities;
using Stockpilot.Domain.Enums;
using Xunit;

namespace Stockpilot.Application.UnitTests.PurchaseOrders;

public class AddPurchaseOrderHandlerTests
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddPurchaseOrderHandlerTests()
    {
        _purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        _purchaseOrderItemRepository = Substitute.For<IPurchaseOrderItemRepository>();
        _supplierRepository = Substitute.For<ISupplierRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
    }

    [Fact]
    public async Task Handle_WhenPurchaseOrderIsValid_ShouldCreateDraftPurchaseOrder()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();
        PurchaseOrder? addedPurchaseOrder = null;
        List<PurchaseOrderItem>? addedPurchaseOrderItems = null;

        _purchaseOrderRepository
            .IsPurchaseOrderExistsAsync("PO-1001", Arg.Any<CancellationToken>())
            .Returns(false);
        _supplierRepository
            .GetSupplierByIdAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns(new Supplier { Id = supplierId });
        _productRepository
            .GetProductByIdAsync(firstProductId, Arg.Any<CancellationToken>())
            .Returns(new Product { Id = firstProductId });
        _productRepository
            .GetProductByIdAsync(secondProductId, Arg.Any<CancellationToken>())
            .Returns(new Product { Id = secondProductId });

        _purchaseOrderRepository.AddPurchaseOrder(
            Arg.Do<PurchaseOrder>(purchaseOrder => addedPurchaseOrder = purchaseOrder),
            Arg.Any<CancellationToken>());
        _purchaseOrderItemRepository.AddPurchaseOrderItem(
            Arg.Do<List<PurchaseOrderItem>>(items => addedPurchaseOrderItems = items),
            Arg.Any<CancellationToken>());

        var request = new AddPurchaseOrderCommand(new AddPurchaseOrderDto
        {
            SupplierId = supplierId,
            Supplier = "Test Supplier",
            OrderNumber = "PO-1001",
            Status = PurchaseOrderStatus.Received,
            SubmittedAt = DateTimeOffset.UtcNow,
            ReceivedAt = DateTimeOffset.UtcNow,
            Items = new List<AddPurchaseOrderItemDto>
            {
                new()
                {
                    ProductId = firstProductId,
                    Product = "First Product",
                    QuantityOrdered = 10,
                    QuantityReceived = 5,
                    UnitCost = 12.50m
                },
                new()
                {
                    ProductId = secondProductId,
                    Product = "Second Product",
                    QuantityOrdered = 20,
                    QuantityReceived = 10,
                    UnitCost = 8.75m
                }
            }
        });

        var handler = new AddPurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _purchaseOrderItemRepository,
            _supplierRepository,
            _productRepository,
            _unitOfWork);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        _purchaseOrderRepository.Received(1).AddPurchaseOrder(
            Arg.Any<PurchaseOrder>(),
            Arg.Any<CancellationToken>());
        _purchaseOrderItemRepository.Received(1).AddPurchaseOrderItem(
            Arg.Any<List<PurchaseOrderItem>>(),
            Arg.Any<CancellationToken>());

        addedPurchaseOrder.Should().NotBeNull();
        addedPurchaseOrder!.Status.Should().Be(PurchaseOrderStatus.Draft);
        addedPurchaseOrder.SubmittedAt.Should().BeNull();
        addedPurchaseOrder.ReceivedAt.Should().BeNull();

        addedPurchaseOrderItems.Should().NotBeNull();
        addedPurchaseOrderItems!.Should().OnlyContain(item => item.QuantityReceived == 0);
        addedPurchaseOrderItems.Should().Contain(item =>
            item.ProductId == firstProductId &&
            item.QuantityOrdered == 10 &&
            item.UnitCost == 12.50m);
        addedPurchaseOrderItems.Should().Contain(item =>
            item.ProductId == secondProductId &&
            item.QuantityOrdered == 20 &&
            item.UnitCost == 8.75m);

        await _unitOfWork.Received(1).SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSupplierDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _purchaseOrderRepository
            .IsPurchaseOrderExistsAsync("PO-1002", Arg.Any<CancellationToken>())
            .Returns(false);
        _supplierRepository
            .GetSupplierByIdAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns((Supplier?)null);

        var request = new AddPurchaseOrderCommand(new AddPurchaseOrderDto
        {
            SupplierId = supplierId,
            Supplier = "Missing Supplier",
            OrderNumber = "PO-1002",
            Items = new List<AddPurchaseOrderItemDto>
            {
                new()
                {
                    ProductId = productId,
                    Product = "Test Product",
                    QuantityOrdered = 10,
                    UnitCost = 12.50m
                }
            }
        });

        var handler = new AddPurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _purchaseOrderItemRepository,
            _supplierRepository,
            _productRepository,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.NotFoundExceptions>();

        _purchaseOrderRepository.DidNotReceive().AddPurchaseOrder(
            Arg.Any<PurchaseOrder>(),
            Arg.Any<CancellationToken>());
        _purchaseOrderItemRepository.DidNotReceive().AddPurchaseOrderItem(
            Arg.Any<List<PurchaseOrderItem>>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _purchaseOrderRepository
            .IsPurchaseOrderExistsAsync("PO-1003", Arg.Any<CancellationToken>())
            .Returns(false);
        _supplierRepository
            .GetSupplierByIdAsync(supplierId, Arg.Any<CancellationToken>())
            .Returns(new Supplier { Id = supplierId });
        _productRepository
            .GetProductByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var request = new AddPurchaseOrderCommand(new AddPurchaseOrderDto
        {
            SupplierId = supplierId,
            Supplier = "Test Supplier",
            OrderNumber = "PO-1003",
            Items = new List<AddPurchaseOrderItemDto>
            {
                new()
                {
                    ProductId = productId,
                    Product = "Missing Product",
                    QuantityOrdered = 10,
                    UnitCost = 12.50m
                }
            }
        });

        var handler = new AddPurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _purchaseOrderItemRepository,
            _supplierRepository,
            _productRepository,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.NotFoundExceptions>();

        _purchaseOrderRepository.DidNotReceive().AddPurchaseOrder(
            Arg.Any<PurchaseOrder>(),
            Arg.Any<CancellationToken>());
        _purchaseOrderItemRepository.DidNotReceive().AddPurchaseOrderItem(
            Arg.Any<List<PurchaseOrderItem>>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderNumberAlreadyExists_ShouldThrowBadRequestException()
    {
        // Arrange
        var supplierId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _purchaseOrderRepository
            .IsPurchaseOrderExistsAsync("PO-1004", Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new AddPurchaseOrderCommand(new AddPurchaseOrderDto
        {
            SupplierId = supplierId,
            Supplier = "Test Supplier",
            OrderNumber = "PO-1004",
            Items = new List<AddPurchaseOrderItemDto>
            {
                new()
                {
                    ProductId = productId,
                    Product = "Test Product",
                    QuantityOrdered = 10,
                    UnitCost = 12.50m
                }
            }
        });

        var handler = new AddPurchaseOrderCommandHandler(
            _purchaseOrderRepository,
            _purchaseOrderItemRepository,
            _supplierRepository,
            _productRepository,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.BadRequestExceptions>();

        _purchaseOrderRepository.DidNotReceive().AddPurchaseOrder(
            Arg.Any<PurchaseOrder>(),
            Arg.Any<CancellationToken>());
        _purchaseOrderItemRepository.DidNotReceive().AddPurchaseOrderItem(
            Arg.Any<List<PurchaseOrderItem>>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }
}
