namespace Stockpilot.Application.Features.PurchaseOrderItem.Dto;

public class GetPurchaseOrderItemByIdDto
{
    public Guid Id { get; set; }
    public string Product { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
}
