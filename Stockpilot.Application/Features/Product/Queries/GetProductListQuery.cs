using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Product.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Product.Queries;

public record GetProductListQuery() : IRequest<IReadOnlyList<GetProductListDto>>;

internal sealed class GetProductListQueryHandler : IRequestHandler<GetProductListQuery, IReadOnlyList<GetProductListDto>>
{
    private readonly IProductRepository _productRepository;
    public GetProductListQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<IReadOnlyList<GetProductListDto>> Handle(GetProductListQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetProductListAsync(cancellationToken);

        if (products == null || !products.Any())
        {
            throw new NotFoundExceptions("No products found.");
        }

        return products.Select(product => new GetProductListDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Category = product.Category?.Name ?? string.Empty,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            QuantityOnHand = product.QuantityOnHand,
            ReorderLevel = product.ReorderLevel,
            IsActive = product.IsActive
        }).ToList();
    }
}