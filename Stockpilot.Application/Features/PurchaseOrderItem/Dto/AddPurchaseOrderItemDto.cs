
namespace Stockpilot.Application.Features.PurchaseOrderItem.Dto;

public class AddPurchaseOrderItemDto
{
    public Guid ProductId { get; set; }
    public string Product { get; set; } = null!;
    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
}
