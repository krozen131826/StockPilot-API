using Stockpilot.Domain.Common;
using Stockpilot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Domain.Entities;

public class PurchaseOrder : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public string OrderNumber { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; }
        = new List<PurchaseOrderItem>();

    public ICollection<StockMovement> StockMovements
    { get; set; } = new List<StockMovement>();
}