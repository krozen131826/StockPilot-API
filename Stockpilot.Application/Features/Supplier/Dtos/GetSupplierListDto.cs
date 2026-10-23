using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Supplier.Dtos;

public class GetSupplierListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}
