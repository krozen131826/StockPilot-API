using Microsoft.EntityFrameworkCore;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Features.Supplier.Dtos;
using Stockpilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Persistence.Repository;

public class SupplierRepository : ISuppllierRepository
{
    private readonly StockpilotDbContext _context;

    public SupplierRepository(StockpilotDbContext context)
    {
        _context = context;
    }

    public void AddSupplier(Supplier supplier, CancellationToken cancellationToken = default)
    {
          _context.Suppliers.AddAsync(supplier, cancellationToken);
    }

    public async Task<Supplier?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
           return await _context.Suppliers.FindAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<Supplier>> GetSupplierListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers.ToListAsync(cancellationToken);
    }

    public Task<bool> IsSupplierExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return _context.Suppliers.AnyAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);
    }
}
