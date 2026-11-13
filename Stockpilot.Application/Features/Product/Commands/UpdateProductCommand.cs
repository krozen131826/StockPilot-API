using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Product.Dtos;

namespace Stockpilot.Application.Features.Product.Commands;

public record UpdateProductCommand(Guid id, UpdateProductDto request) : IRequest<Unit>;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.request.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

        RuleFor(x => x.request.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

        RuleFor(x => x.request.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be greater than or equal to 0.");

        RuleFor(x => x.request.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price must be greater than or equal to 0.");

        RuleFor(x => x.request.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level must be greater than or equal to 0.");
    }
}


internal sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateProductCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.Errors.Count > 0)
        {
            throw new ValidationExceptions(validationResult);
        }
        var product = await _productRepository.GetProductByIdAsync(request.id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundExceptions($"Product with ID '{request.id}' not found.");
        }

        product.CategoryId = request.request.CategoryId;
        product.SKU = request.request.SKU;
        product.Name = request.request.Name;
        product.Description = request.request.Description;
        product.CostPrice = request.request.CostPrice;
        product.SellingPrice = request.request.SellingPrice;
        product.ReorderLevel = request.request.ReorderLevel;
        product.IsActive = request.request.IsActive;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}