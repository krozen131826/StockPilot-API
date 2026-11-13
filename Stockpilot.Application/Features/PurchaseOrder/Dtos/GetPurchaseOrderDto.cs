using Stockpilot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.PurchaseOrder.Dtos;

public class GetPurchaseOrderDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Supplier { get; set; } = null!;
    public string OrderNumber { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public string? Notes { get; set; }
}
