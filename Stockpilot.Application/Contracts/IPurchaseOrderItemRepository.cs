using Stockpilot.Application.Features.PurchaseOrderItem.Dto;
using Stockpilot.Domain.Entities;

namespace Stockpilot.Application.Contracts;

public interface IPurchaseOrderItemRepository
{
    void AddPurchaseOrderItem(List<PurchaseOrderItem> purchaseOrderItem, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseOrderItem>> GetPurchaseOrderItemsListAsync(Guid purchaseOrderId, CancellationToken cancellationToken = default);
    Task<PurchaseOrderItem?> GetPurchaseOrderItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
