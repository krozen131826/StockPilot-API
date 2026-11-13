using Stockpilot.Domain.Entities;

namespace Stockpilot.Application.Contracts;

public interface IPurchaseOrderRepository
{
    void AddPurchaseOrder(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
    Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrderListAsync(CancellationToken cancellationToken = default);
    Task<bool> IsPurchaseOrderExistsAsync(string orderNumber, CancellationToken cancellationToken = default);
}
