using Stockpilot.Application.Features.PurchaseOrderItem.Dto;
using Stockpilot.Domain.Enums;

namespace Stockpilot.Application.Features.PurchaseOrder.Dtos;

public class AddPurchaseOrderDto
{
    public Guid SupplierId { get; set; }
    public string Supplier { get; set; } = null!;
    public string OrderNumber { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public string? Notes { get; set; }
    public ICollection<AddPurchaseOrderItemDto> Items { get; set; } = new List<AddPurchaseOrderItemDto>();
}
