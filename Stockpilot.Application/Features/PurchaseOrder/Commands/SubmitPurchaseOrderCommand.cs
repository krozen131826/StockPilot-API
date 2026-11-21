using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Domain.Enums;

namespace Stockpilot.Application.Features.PurchaseOrder.Commands;

public record SubmitPurchaseOrderCommand(Guid id) : IRequest<Unit>;

internal sealed class SubmitPurchaseOrderCommandValidator : AbstractValidator<SubmitPurchaseOrderCommand>
{
    public SubmitPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.id)
            .NotEmpty()
            .WithMessage("Purchase Order ID is required.");
    }
}

internal sealed class SubmitPurchaseOrderCommandHandler : IRequestHandler<SubmitPurchaseOrderCommand, Unit>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitPurchaseOrderCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SubmitPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var validator = new SubmitPurchaseOrderCommandValidator();

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

        if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
        {
            throw new BadRequestExceptions("Only draft purchase orders can be submitted.");
        }

        if (purchaseOrder.Items is null || !purchaseOrder.Items.Any())
        {
            throw new BadRequestExceptions("A purchase order must contain at least one item.");
        }

        purchaseOrder.Status = PurchaseOrderStatus.Submitted;
        purchaseOrder.SubmittedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
