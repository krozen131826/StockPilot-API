using FluentAssertions;
using NSubstitute;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Features.StockMovement.Command;
using Stockpilot.Application.Features.StockMovement.Dtos;
using Stockpilot.Domain.Entities;
using Stockpilot.Domain.Enums;
using Xunit;

namespace Stockpilot.Application.UnitTests.StockMovements;

public class AddStockMovementHandlerTests
{
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AddStockMovementHandlerTests()
    {
        _stockMovementRepository = Substitute.For<IStockMovementRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
    }

    [Fact]
    public async Task Handle_WhenStockInIsSuccessful_ShouldIncreaseProductStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            QuantityOnHand = 35
        };

        _productRepository
            .GetProductByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _currentUserService.FullName.Returns("Test User");

        var request = new AddStockMovementCommand(
            productId,
            new AddStockMovementDto
            {
                QuantityChange = 10,
                Reason = "Receiving stock"
            });

        var handler = new AddStockMovementCommandHandler(
            _stockMovementRepository,
            _productRepository,
            _currentUserService,
            _unitOfWork);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        product.QuantityOnHand.Should().Be(45);

        _stockMovementRepository.Received(1).AddStockMovement(
            Arg.Is<StockMovement>(movement =>
                movement.ProductId == productId &&
                movement.QuantityChange == 10 &&
                movement.QuantityBefore == 35 &&
                movement.QuantityAfter == 45 &&
                movement.MovementType == StockMovementType.StockIn &&
                movement.PerformedByUserId == userId &&
                movement.PerformedByUser == "Test User"),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _productRepository
            .GetProductByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var request = new AddStockMovementCommand(
            productId,
            new AddStockMovementDto
            {
                QuantityChange = 10,
                Reason = "Receiving stock"
            });

        var handler = new AddStockMovementCommandHandler(
            _stockMovementRepository,
            _productRepository,
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
    public async Task Handle_WhenStockInQuantityIsZero_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var request = new AddStockMovementCommand(
            productId,
            new AddStockMovementDto
            {
                QuantityChange = 0,
                Reason = "Receiving stock"
            });

        var handler = new AddStockMovementCommandHandler(
            _stockMovementRepository,
            _productRepository,
            _currentUserService,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.ValidationExceptions>();

        await _productRepository.DidNotReceive().GetProductByIdAsync(
            productId,
            Arg.Any<CancellationToken>());

        _stockMovementRepository.DidNotReceive().AddStockMovement(
            Arg.Any<StockMovement>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStockInQuantityIsNegative_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var request = new AddStockMovementCommand(
            productId,
            new AddStockMovementDto
            {
                QuantityChange = -10,
                Reason = "Receiving stock"
            });

        var handler = new AddStockMovementCommandHandler(
            _stockMovementRepository,
            _productRepository,
            _currentUserService,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.ValidationExceptions>();

        await _productRepository.DidNotReceive().GetProductByIdAsync(
            productId,
            Arg.Any<CancellationToken>());

        _stockMovementRepository.DidNotReceive().AddStockMovement(
            Arg.Any<StockMovement>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStockInReasonIsEmpty_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var request = new AddStockMovementCommand(
            productId,
            new AddStockMovementDto
            {
                QuantityChange = 10,
                Reason = string.Empty
            });

        var handler = new AddStockMovementCommandHandler(
            _stockMovementRepository,
            _productRepository,
            _currentUserService,
            _unitOfWork);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Stockpilot.Application.Exceptions.ValidationExceptions>();

        await _productRepository.DidNotReceive().GetProductByIdAsync(
            productId,
            Arg.Any<CancellationToken>());

        _stockMovementRepository.DidNotReceive().AddStockMovement(
            Arg.Any<StockMovement>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.DidNotReceive().SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }
}
