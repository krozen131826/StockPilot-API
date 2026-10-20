using Stockpilot.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Domain.Entities;

public class Product : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }

    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PurchaseOrderItem> PurchaseOrderItems
    { get; set; } = new List<PurchaseOrderItem>();

    public ICollection<StockMovement> StockMovements
    { get; set; } = new List<StockMovement>();
}