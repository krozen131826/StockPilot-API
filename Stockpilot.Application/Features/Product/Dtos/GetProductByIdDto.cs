namespace Stockpilot.Application.Features.Product.Dtos;

public class GetProductByIdDto
{
    public Guid CategoryId { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal CostPrice { get; set; }

    public decimal SellingPrice { get; set; }

    public int QuantityOnHand { get; set; }

    public int ReorderLevel { get; set; }

    public bool IsActive { get; set; } = true;
}
