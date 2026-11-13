using Microsoft.EntityFrameworkCore;
using Stockpilot.Application.Contracts;
using Stockpilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Persistence.Repository;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly StockpilotDbContext _context;

    public PurchaseOrderRepository(StockpilotDbContext context)
    {
        _context = context;
    }

    public void AddPurchaseOrder(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
    {
        _context.PurchaseOrders.Add(purchaseOrder);
    }

    public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders
        .Include(po => po.Supplier)
        .Include(po => po.Items)
        .ThenInclude(item => item.Product).FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrderListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders.AsNoTracking().Include(i => i.Supplier).ToListAsync(cancellationToken);
    }

    public async Task<bool> IsPurchaseOrderExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrders.AnyAsync(p => p.OrderNumber == orderNumber, cancellationToken);
    }

}
