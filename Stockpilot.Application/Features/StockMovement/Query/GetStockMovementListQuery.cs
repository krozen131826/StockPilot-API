using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Features.StockMovement.Dtos;
using Stockpilot.Domain.Entities;
using Stockpilot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.StockMovement.Query;

public record GetStockMovementListQuery() : IRequest<IReadOnlyList<GetStockMovementListDto>>;

internal sealed class GetStockMovementListQueryHandler : IRequestHandler<GetStockMovementListQuery, IReadOnlyList<GetStockMovementListDto>>
{
    private readonly IStockMovementRepository _stockMovementRepository;
    public GetStockMovementListQueryHandler(IStockMovementRepository stockMovementRepository)
    {
        _stockMovementRepository = stockMovementRepository;
    }
    public async Task<IReadOnlyList<GetStockMovementListDto>> Handle(GetStockMovementListQuery request, CancellationToken cancellationToken)
    {
        var stockMovements = await _stockMovementRepository.GetStockMovementListAsync(cancellationToken);

        return stockMovements.Select(sm => new GetStockMovementListDto
        {
            Product = sm.Product.Name,
            PerformedByUser = sm.PerformedByUser,
            MovementType = sm.MovementType,
            QuantityChange = sm.QuantityChange,
            QuantityBefore = sm.QuantityBefore,
            QuantityAfter = sm.QuantityAfter,
            Reason = sm.Reason,
            Date = sm.CreatedDate
        }).ToList();
    }
}
