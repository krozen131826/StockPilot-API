using Microsoft.AspNetCore.Identity;
using Stockpilot.Domain.Entities;

namespace Stockpilot.Identity.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<PurchaseOrder> CreatedPurchaseOrders
    { get; set; } = new List<PurchaseOrder>();

    public ICollection<StockMovement> StockMovements
    { get; set; } = new List<StockMovement>();
}