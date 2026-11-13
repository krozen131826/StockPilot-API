using Stockpilot.Domain.Entities;

namespace Stockpilot.Application.Contracts;

public interface IStockMovementRepository
{
    void AddStockMovement(StockMovement stockMovement, CancellationToken cancellationToken);
    Task<IReadOnlyList<StockMovement>> GetStockMovementListAsync(CancellationToken cancellationToken);
}