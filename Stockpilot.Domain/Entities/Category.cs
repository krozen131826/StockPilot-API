using Stockpilot.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Domain.Entities;

public class Category : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; }
        = new List<Product>();
}