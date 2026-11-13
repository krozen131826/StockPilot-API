
using Stockpilot.Domain.Enums;

namespace Stockpilot.Application.Features.StockMovement.Dtos;

public class GetStockMovementListDto
{
    public string Product { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; } 
    public int QuantityChange { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string PerformedByUser { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
