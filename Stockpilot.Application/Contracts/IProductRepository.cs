using Stockpilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Contracts;

public interface IProductRepository
{
    Task<bool> ExistsByNameAsync(
       string name,
       CancellationToken cancellationToken = default);

    void AddProduct(
        Product product,
        CancellationToken cancellationToken = default);


    Task<Product?> GetProductByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetProductListAsync(
        CancellationToken cancellationToken = default);
}
