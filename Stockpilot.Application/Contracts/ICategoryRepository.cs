using Stockpilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Contracts;

public interface ICategoryRepository
{
    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    void AddCategory(
        Category category,
        CancellationToken cancellationToken = default);


    Task<Category?> GetCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> GetCategoryListAsync(
        CancellationToken cancellationToken = default);
}