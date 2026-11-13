using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.StockMovement.Dtos;

public class AdjustStockMovementDto
{
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
}
