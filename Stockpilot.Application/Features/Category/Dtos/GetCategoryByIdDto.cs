using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Category.Dtos;

public class GetCategoryByIdDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
