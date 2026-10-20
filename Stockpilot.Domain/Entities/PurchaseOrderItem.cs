using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Domain.Entities;

public class PurchaseOrderItem
{
    public Guid Id { get; set; }

    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
}