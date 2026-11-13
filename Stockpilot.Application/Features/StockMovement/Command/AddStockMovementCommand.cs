using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.StockMovement.Dtos;

namespace Stockpilot.Application.Features.StockMovement.Command;

public record AddStockMovementCommand(Guid productId, AddStockMovementDto request) : IRequest<Unit>;
    
internal sealed class AddStockMovementCommandValidator : AbstractValidator<AddStockMovementCommand>
{
    public AddStockMovementCommandValidator()
    {
        RuleFor(x => x.request.QuantityChange)
            .NotEmpty().WithMessage("Quantity change is required.")
            .GreaterThan(0).WithMessage("Quantity change must be greater than 0.");

        RuleFor(x => x.request.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(200).WithMessage("Reason must not exceed 200 characters.");
    }
}

internal sealed class AddStockMovementCommandHandler : IRequestHandler<AddStockMovementCommand, Unit>
{
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IProductRepository _productRepository;

    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    public AddStockMovementCommandHandler(IStockMovementRepository stockMovementRepository, IProductRepository productRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _stockMovementRepository = stockMovementRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;

    }
    public async Task<Unit> Handle(AddStockMovementCommand request, CancellationToken cancellationToken)
    {
        var validator = new AddStockMovementCommandValidator();

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


        if (product.QuantityOnHand + request.request.QuantityChange < 0)
        {
            throw new BadRequestExceptions("Stock level cannot be negative.");
        }

        if (_currentUserService is null)
        {
            throw new UnAuthorizedExceptions("User is not authenticated.");
        }

        var stockMovement = new Domain.Entities.StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = request.productId,
            QuantityChange = request.request.QuantityChange,
            QuantityBefore = product.QuantityOnHand,
            QuantityAfter = product.QuantityOnHand + request.request.QuantityChange,
            MovementType = Domain.Enums.StockMovementType.StockIn,
            Reason = request.request.Reason,
            CreatedDate = DateTime.UtcNow,
            PerformedByUserId = _currentUserService.UserId, 
            PerformedByUser = _currentUserService.FullName,
        };

        product.QuantityOnHand += request.request.QuantityChange;

        _stockMovementRepository.AddStockMovement(stockMovement, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}