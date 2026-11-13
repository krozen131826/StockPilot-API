using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.PurchaseOrder.Dtos;
using Stockpilot.Application.Features.PurchaseOrderItem.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.PurchaseOrder.Queries;

public record GetPurchaseOrderByIdQuery(Guid purchaseOrderId) : IRequest<GetPurchaseOrderByIdDto>;

internal sealed class GetPurchaseOrderByIdQueryValidator : AbstractValidator<GetPurchaseOrderByIdQuery>
{
    public GetPurchaseOrderByIdQueryValidator()
    {
        RuleFor(x => x.purchaseOrderId).NotEmpty().WithMessage("Purchase Order Id is required.");
    }
}   

internal sealed class GetPurchaseOrderByIdQueryHandler : IRequestHandler<GetPurchaseOrderByIdQuery, GetPurchaseOrderByIdDto>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    public GetPurchaseOrderByIdQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
    }
    public async Task<GetPurchaseOrderByIdDto> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetPurchaseOrderByIdQueryValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count() > 0)
        {
            throw new ValidationExceptions(validationResult);
        }

        var purchaseOrder = await _purchaseOrderRepository.GetPurchaseOrderByIdAsync(request.purchaseOrderId, cancellationToken);

        if (purchaseOrder is null)
        {
            throw new NotFoundExceptions($"No purchase order found with Id: {request.purchaseOrderId}");
        }

        return new GetPurchaseOrderByIdDto
        {
            Id = purchaseOrder.Id,
            SupplierId = purchaseOrder.SupplierId,
            Supplier = purchaseOrder.Supplier.Name,
            OrderNumber = purchaseOrder.OrderNumber,
            Status = purchaseOrder.Status,
            SubmittedAt = purchaseOrder.SubmittedAt,
            ReceivedAt = purchaseOrder.ReceivedAt,
            Notes = purchaseOrder.Notes,
            Items = purchaseOrder.Items.Select(poi => new GetPurchaseOrderItemDto
            {
                Id = poi.Id,
                PurchaseOrderId = poi.PurchaseOrderId,
                ProductId = poi.ProductId,
                Product = poi.Product.Name,
                QuantityOrdered = poi.QuantityOrdered,
                QuantityReceived = poi.QuantityReceived,
                UnitCost = poi.UnitCost
            }).ToList()
        };
    }
}