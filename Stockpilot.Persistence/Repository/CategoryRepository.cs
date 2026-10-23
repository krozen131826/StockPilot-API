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

    public void AddCategory(
        Category category,
        CancellationToken cancellationToken = default)
    {
         _context.Categories.AddAsync(
            category,
            cancellationToken);
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FindAsync(id,cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetCategoryListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories.ToListAsync(cancellationToken);
    }
}