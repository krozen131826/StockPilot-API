
namespace Stockpilot.Application.Features.StockMovement.Dtos;

public class AddStockMovementDto
{
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
}
