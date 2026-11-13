using Microsoft.EntityFrameworkCore;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;

namespace Stockpilot.Persistence.Repository;


public class UnitOfWork : IUnitOfWork
{
    private readonly StockpilotDbContext _context;

    public UnitOfWork(StockpilotDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictExceptions(
                "The product's stock was updated by another user. Reload the latest stock before retrying.");
        }
    }
}