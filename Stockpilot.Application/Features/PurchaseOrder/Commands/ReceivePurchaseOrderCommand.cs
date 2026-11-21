using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Domain.Enums;

namespace Stockpilot.Application.Features.PurchaseOrder.Commands;

public record ReceivePurchaseOrderCommand(Guid id) : IRequest<Unit>;

internal sealed class ReceivePurchaseOrderCommandValidator : AbstractValidator<ReceivePurchaseOrderCommand>
{
    public ReceivePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.id)
            .NotEmpty()
            .WithMessage("Purchase Order ID is required.");
    }
}

internal sealed class ReceivePurchaseOrderCommandHandler : IRequestHandler<ReceivePurchaseOrderCommand, Unit>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ReceivePurchaseOrderCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IProductRepository productRepository,
        IStockMovementRepository stockMovementRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _productRepository = productRepository;
        _stockMovementRepository = stockMovementRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ReceivePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var validator = new ReceivePurchaseOrderCommandValidator();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count() > 0)
        {
            throw new ValidationExceptions(validationResult);
        }

        var purchaseOrder = await _purchaseOrderRepository.GetPurchaseOrderByIdAsync(request.id, cancellationToken);

        if (purchaseOrder is null)
        {
            throw new NotFoundExceptions($"Purchase order with ID '{request.id}' not found.");
        }

        if (purchaseOrder.Status != PurchaseOrderStatus.Submitted)
        {
            throw new BadRequestExceptions("Only submitted purchase orders can be received.");
        }

        if (purchaseOrder.Items is null || !purchaseOrder.Items.Any())
        {
            throw new BadRequestExceptions("A purchase order must contain at least one item.");
        }

        foreach (var item in purchaseOrder.Items)
        {
            var product = item.Product ?? await _productRepository.GetProductByIdAsync(item.ProductId, cancellationToken);

            if (product is null)
            {
                throw new NotFoundExceptions($"Product with ID '{item.ProductId}' not found.");
            }

            var quantityBefore = product.QuantityOnHand;
            var quantityAfter = quantityBefore + item.QuantityOrdered;

            product.QuantityOnHand = quantityAfter;
            item.QuantityReceived = item.QuantityOrdered;

            var stockMovement = new Domain.Entities.StockMovement
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                PurchaseOrderId = purchaseOrder.Id,
                QuantityChange = item.QuantityOrdered,
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter,
                MovementType = StockMovementType.StockIn,
                Reason = $"Purchase order '{purchaseOrder.OrderNumber}' received.",
                CreatedDate = DateTime.UtcNow,
                PerformedByUserId = _currentUserService.UserId,
                PerformedByUser = _currentUserService.FullName
            };

            _stockMovementRepository.AddStockMovement(stockMovement, cancellationToken);
        }

        purchaseOrder.Status = PurchaseOrderStatus.Received;
        purchaseOrder.ReceivedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
