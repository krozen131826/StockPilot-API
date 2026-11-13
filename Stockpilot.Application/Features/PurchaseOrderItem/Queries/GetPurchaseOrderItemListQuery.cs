using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.PurchaseOrderItem.Dto;

namespace Stockpilot.Application.Features.PurchaseOrderItem.Queries;

public record GetPurchaseOrderItemListQuery(Guid purchaseOrderId) : IRequest<IReadOnlyList<GetPurchaseOrderItemDto>>;

internal sealed class GetPurchaseOrderItemListQueryValidator : AbstractValidator<GetPurchaseOrderItemListQuery>
{
    public GetPurchaseOrderItemListQueryValidator()
    {
        RuleFor(x => x.purchaseOrderId).NotEmpty().WithMessage("Purchase Order Id is required.");
    }
}

internal sealed class GetPurchaseOrderItemListQueryHandler : IRequestHandler<GetPurchaseOrderItemListQuery, IReadOnlyList<GetPurchaseOrderItemDto>>
{
    private readonly IPurchaseOrderItemRepository _purchaseOrderItemRepository;
    public GetPurchaseOrderItemListQueryHandler(IPurchaseOrderItemRepository purchaseOrderItemRepository)
    {
        _purchaseOrderItemRepository = purchaseOrderItemRepository;
    }
    public async Task<IReadOnlyList<GetPurchaseOrderItemDto>> Handle(GetPurchaseOrderItemListQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetPurchaseOrderItemListQueryValidator();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count() > 0)
        {
            throw new ValidationExceptions(validationResult);
        }
        var purchaseOrderItems = await _purchaseOrderItemRepository.GetPurchaseOrderItemsListAsync(request.purchaseOrderId, cancellationToken);

        if (purchaseOrderItems.Any() || purchaseOrderItems is null)
        {
            throw new NotFoundExceptions("No purchase order items found.");
        }

        return purchaseOrderItems.Select(poi => new GetPurchaseOrderItemDto
        {
            Id = poi.Id,
            PurchaseOrderId = poi.PurchaseOrderId,
            ProductId = poi.ProductId,
            Product = poi.Product.Name,
            QuantityOrdered = poi.QuantityOrdered,
            QuantityReceived = poi.QuantityReceived,
            UnitCost = poi.UnitCost
        }).ToList();
    }
}