using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Product.Commands;

public record DeleteProductCommand(Guid id) : IRequest<Unit>;

internal sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.id)
            .NotEmpty().WithMessage("Product ID is required.");
    }
}   

internal sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var validator = new DeleteProductCommandValidator();
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

        product.IsActive = false;
        product.IsDeleted = true;
        product.LastModifiedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}