using Stockpilot.Application.Features.PurchaseOrderItem.Dto;
using Stockpilot.Domain.Enums;


namespace Stockpilot.Application.Features.PurchaseOrder.Dtos;

public class GetPurchaseOrderByIdDto
{
   
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public PurchaseOrderStatus Status { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public DateTimeOffset? ReceivedAt { get; set; }
        public string? Notes { get; set; }
        public IReadOnlyList<GetPurchaseOrderItemDto> Items { get; set; } = new List<GetPurchaseOrderItemDto>();
}
