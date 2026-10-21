using Stockpilot.Application.Contracts;

namespace Stockpilot.Persistence.Repository;


public class UnitOfWork : IUnitOfWork
{
    private readonly StockpilotDbContext _context;

    public UnitOfWork(StockpilotDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}