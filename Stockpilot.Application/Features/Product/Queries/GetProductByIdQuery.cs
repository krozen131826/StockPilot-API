using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Product.Dtos;

namespace Stockpilot.Application.Features.Product.Queries;

public record GetProductByIdQuery(Guid id) : IRequest<GetProductByIdDto>;

internal sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdDto>
{
    private readonly IProductRepository _productRepository;
    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<GetProductByIdDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductByIdAsync(request.id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundExceptions($"Product with ID '{request.id}' not found.");
        }

        return new GetProductByIdDto
        {
            CategoryId = product.CategoryId,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            QuantityOnHand = product.QuantityOnHand,
            ReorderLevel = product.ReorderLevel,
            IsActive = product.IsActive
        };
    }
}