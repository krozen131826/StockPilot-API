using Microsoft.EntityFrameworkCore;
using Stockpilot.Application.Contracts;
using Stockpilot.Domain.Entities;

namespace Stockpilot.Persistence.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly StockpilotDbContext _context;

    public CategoryRepository(StockpilotDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AnyAsync(
                x => x.Name.ToLower() == name.ToLower(),
                cancellationToken);
    }

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(
            category,
            cancellationToken);
    }
}