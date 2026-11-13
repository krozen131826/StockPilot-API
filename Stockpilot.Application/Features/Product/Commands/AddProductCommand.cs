using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Product.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Product.Commands;

public record AddProductCommand(AddProductDto request) : IRequest<Guid>;

internal sealed class AddProductCommandValidator : AbstractValidator<AddProductCommand>
{
    public AddProductCommandValidator()
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

internal sealed class AddProductCommandHandler : IRequestHandler<AddProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    public AddProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Guid> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        var validator = new AddProductCommandValidator();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new ValidationExceptions(validationResult);
        }

        if (await _productRepository.ExistsByNameAsync(request.request.Name, cancellationToken))
        {
            throw new BadRequestExceptions($"A product with the name '{request.request.Name}' already exists.");
        }

        var product = new Domain.Entities.Product
        {
            CategoryId = request.request.CategoryId,
            SKU = request.request.SKU,
            Name = request.request.Name,
            Description = request.request.Description,
            CostPrice = request.request.CostPrice,
            SellingPrice = request.request.SellingPrice,
            ReorderLevel = request.request.ReorderLevel,
            IsActive = request.request.IsActive
        };

        _productRepository.AddProduct(product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}