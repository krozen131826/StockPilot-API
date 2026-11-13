using FluentValidation;
using FluentValidation.Validators;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.StockMovement.Dtos;
using Stockpilot.Domain.Entities;
using Stockpilot.Domain.Enums;


namespace Stockpilot.Application.Features.StockMovement.Command;

public record AdjustmentStockMovementCommand(Guid productId, AdjustStockMovementDto request) : IRequest<Unit>;

internal sealed class AdjustmentStockMovementCommandValidator : AbstractValidator<AdjustmentStockMovementCommand>
{
    public AdjustmentStockMovementCommandValidator()
    {
     
        RuleFor(x => x.request.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(200).WithMessage("Reason must not exceed 200 characters.");
    }
}

internal sealed class AdjustmentStockMovementCommandHandler : IRequestHandler<AdjustmentStockMovementCommand, Unit>
{
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    public AdjustmentStockMovementCommandHandler(IStockMovementRepository stockMovementRepository, IProductRepository productRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _stockMovementRepository = stockMovementRepository;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }
    public async Task<Unit> Handle(AdjustmentStockMovementCommand request, CancellationToken cancellationToken)
    {
        var validator = new AdjustmentStockMovementCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.Errors.Count > 0)
        {
            throw new ValidationExceptions(validationResult);
        }
        var product = await _productRepository.GetProductByIdAsync(request.productId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundExceptions("Product not found.");
        }

        if (_currentUserService is null)
        {
            throw new NotFoundExceptions("User needs to be Authenticated.");
        }

        var quantityChange = request.request.QuantityChange;
        var quantityBefore = product.QuantityOnHand;
        var quantityAfter = quantityBefore + quantityChange;

        // 1. Validate the adjustment
        if (quantityChange == 0)
        {
            throw new BadRequestExceptions(
                "Adjustment quantity cannot be zero.");
        }

        // 2. Prevent negative stock
        if (quantityAfter < 0)
        {
            throw new BadRequestExceptions(
                "Insufficient stock for the adjustment.");
        }

        // 3. Update the product quantity
        product.QuantityOnHand = quantityAfter;

        // 4. Create the stock movement
        var stockMovement = new Domain.Entities.StockMovement
        {
            ProductId = request.productId,
            QuantityChange = quantityChange,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
            Reason = request.request.Reason,
            PerformedByUserId = _currentUserService.UserId,
            PerformedByUser = _currentUserService.FullName,
            CreatedDate = DateTime.UtcNow,
            MovementType = StockMovementType.Adjustment
        };

        _stockMovementRepository.AddStockMovement(stockMovement, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}