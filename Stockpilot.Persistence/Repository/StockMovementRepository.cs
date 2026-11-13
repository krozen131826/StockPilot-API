using Microsoft.EntityFrameworkCore;
using Stockpilot.Application.Contracts;
using Stockpilot.Domain.Entities;


namespace Stockpilot.Persistence.Repository;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly StockpilotDbContext _context;

    public StockMovementRepository(StockpilotDbContext context)
    {
        _context = context;
    }
    public void AddStockMovement(StockMovement stockMovement, CancellationToken cancellationToken)
    {
        _context.StockMovements.Add(stockMovement);
    }

    public async Task<IReadOnlyList<StockMovement>> GetStockMovementListAsync(CancellationToken cancellationToken)
    {
        var results = await _context.StockMovements.AsNoTracking().Include(s => s.Product).ToListAsync(cancellationToken);

        return results;
    }
}
