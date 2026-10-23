
namespace Stockpilot.Application.Features.Category.Dtos;

public class CategoryListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
