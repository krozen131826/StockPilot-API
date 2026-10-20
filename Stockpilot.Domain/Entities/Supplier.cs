using Stockpilot.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Domain.Entities;

public class Supplier : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PurchaseOrder> PurchaseOrders
    { get; set; } = new List<PurchaseOrder>();
}