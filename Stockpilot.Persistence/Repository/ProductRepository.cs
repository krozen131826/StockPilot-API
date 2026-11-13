using Microsoft.EntityFrameworkCore;
using Stockpilot.Application.Contracts;
using Stockpilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Persistence.Repository;

public class ProductRepository : IProductRepository
{
    private readonly StockpilotDbContext _context;

    public ProductRepository(StockpilotDbContext context)
    {
        _context = context;
    }

    public void AddProduct(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
       return await _context.Products.AnyAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
       .Include(p => p.Category)
       .FirstOrDefaultAsync(
           p => p.Id == id,
           cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetProductListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);
    }
}
