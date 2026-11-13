using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.PurchaseOrder.Dtos;
using Stockpilot.Application.Features.PurchaseOrderItem.Dto;
using Stockpilot.Domain.Enums;

namespace Stockpilot.Application.Features.PurchaseOrder.Commands;

public record AddPurchaseOrderCommand(AddPurchaseOrderDto request) : IRequest<Unit>;


internal sealed class AddPurchaseOrderCommandValidator : AbstractValidator<AddPurchaseOrderCommand>
{
    public AddPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.request.SupplierId).NotEmpty().WithMessage("SupplierId is required.");
        RuleFor(x => x.request.Supplier).NotEmpty().WithMessage("Supplier is required.");
        RuleFor(x => x.request.OrderNumber).NotEmpty().WithMessage("OrderNumber is required.");
        RuleFor(x => x.request.Status).IsInEnum().WithMessage("Status must be a valid enum value.");
        RuleFor(x => x.request.Items).NotNull().NotEmpty().WithMessage("A purchase order must contain at least one item.");
        RuleForEach(x => x.request.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Product)
                .NotEmpty()
                .WithMessage("Product is required.");

            item.RuleFor(x => x.QuantityOrdered)
                .GreaterThan(0)
                .WithMessage("QuantityOrdered must be greater than zero.");

            item.RuleFor(x => x.UnitCost)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Unit cost cannot be negative.");

            item.RuleFor(x => x.QuantityReceived)
                .GreaterThanOrEqualTo(0)
                .WithMessage("QuantityReceived cannot be negative.");
        });
    }
}


internal sealed class AddPurchaseOrderCommandHandler : IRequestHandler<AddPurchaseOrderCommand, Unit>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    public AddPurchaseOrderCommandHandler(IPurchaseOrderRepository purchaseOrderRepository, IPurchaseOrderItemRepository purchaseOrderItemRepository, IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _purchaseOrderItemRepository = purchaseOrderItemRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Unit> Handle(AddPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var validator = new AddPurchaseOrderCommandValidator();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count() > 0)
        {
            throw new ValidationExceptions(validationResult);
        }

        var purchaseOrderId = Guid.NewGuid();

        var purchaseOrder = new Domain.Entities.PurchaseOrder
        {
            Id = purchaseOrderId,
            SupplierId = request.request.SupplierId,
            OrderNumber = request.request.OrderNumber,
            Status = request.request.Status,
            SubmittedAt = request.request.SubmittedAt,
            ReceivedAt = request.request.ReceivedAt,
            Notes = request.request.Notes,
        };

        var purchaseOrderItems = request.request.Items.Select(item => new Domain.Entities.PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = purchaseOrderId,
            ProductId = item.ProductId,
            QuantityOrdered = item.QuantityOrdered,
            QuantityReceived = item.QuantityReceived,
            UnitCost = item.UnitCost
        }).ToList();

        _purchaseOrderRepository.AddPurchaseOrder(purchaseOrder, cancellationToken);
        
        _purchaseOrderItemRepository.AddPurchaseOrderItem(purchaseOrderItems, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}