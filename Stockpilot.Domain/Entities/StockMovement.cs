using Stockpilot.Domain.Common;
using Stockpilot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Domain.Entities;

public class StockMovement : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid PerformedByUserId { get; set; }
    public Guid PerformedByUser { get; set; }

    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    public StockMovementType MovementType { get; set; }
    public int QuantityChange { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }

    public string Reason { get; set; } = string.Empty;
}