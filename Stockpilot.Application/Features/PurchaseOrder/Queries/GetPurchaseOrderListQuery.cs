using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.PurchaseOrder.Dtos;

namespace Stockpilot.Application.Features.PurchaseOrder.Queries;

public record GetPurchaseOrderListQuery() : IRequest<IReadOnlyList<GetPurchaseOrderDto>>;

//internal sealed class GetPurchaseOrderListQueryValidator : AbstractValidator<GetPurchaseOrderListQuery>
//{
//    public GetPurchaseOrderListQueryValidator()
//    {
//        RuleFor(x => x.purchaseOrderId).NotEmpty().WithMessage("Purchase Order Id is required.");
//    }
//}

internal sealed class GetPurchaseOrderListQueryHandler : IRequestHandler<GetPurchaseOrderListQuery, IReadOnlyList<GetPurchaseOrderDto>>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    public GetPurchaseOrderListQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
    }
    public async Task<IReadOnlyList<GetPurchaseOrderDto>> Handle(GetPurchaseOrderListQuery request, CancellationToken cancellationToken)
    {
        //var validator = new GetPurchaseOrderListQueryValidator();

        //var validationResult = await validator.ValidateAsync(request, cancellationToken);

        //if (validationResult.Errors.Count() > 0)
        //{
        //    throw new ValidationExceptions(validationResult);
        //}

        var purchaseOrders = await _purchaseOrderRepository.GetPurchaseOrderListAsync(cancellationToken);

        if (purchaseOrders is null || !purchaseOrders.Any())
        {
            throw new NotFoundExceptions("No purchase orders found.");
        }

        return purchaseOrders.Select(po => new GetPurchaseOrderDto
        {
            Id = po.Id,
            SupplierId = po.SupplierId,
            Supplier = po.Supplier.Name,
            OrderNumber = po.OrderNumber,
            Status = po.Status,
            SubmittedAt = po.SubmittedAt,
            ReceivedAt = po.ReceivedAt,
            Notes = po.Notes
        }).ToList();
    }
}
