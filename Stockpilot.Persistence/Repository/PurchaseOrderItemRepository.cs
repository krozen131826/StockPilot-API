using Microsoft.EntityFrameworkCore;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Features.PurchaseOrderItem.Dto;
using Stockpilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Persistence.Repository;

public class PurchaseOrderItemRepository : IPurchaseOrderItemRepository
{
    private readonly StockpilotDbContext _context;

    public PurchaseOrderItemRepository(StockpilotDbContext context)
    {
        _context = context;
    }
    public void AddPurchaseOrderItem(List<PurchaseOrderItem> purchaseOrderItems, CancellationToken cancellationToken = default)
    {
        _context.PurchaseOrderItems.AddRange(purchaseOrderItems);
    }

    public async Task<PurchaseOrderItem?> GetPurchaseOrderItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
       return await _context.PurchaseOrderItems.FindAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<PurchaseOrderItem>> GetPurchaseOrderItemsListAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default)
    {
        return await _context.PurchaseOrderItems.Include(p => p.Product).Where(p => p.PurchaseOrderId == purchaseOrderId).AsNoTracking().ToListAsync(cancellationToken);
    }
}
