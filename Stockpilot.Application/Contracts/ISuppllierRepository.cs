using Stockpilot.Application.Features.Supplier.Dtos;
using Stockpilot.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Contracts;

public interface ISuppllierRepository
{
    Task<bool> IsSupplierExistsAsync(string name, CancellationToken cancellationToken = default);

    void AddSupplier(Supplier supplier, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Supplier>> GetSupplierListAsync(CancellationToken cancellationToken = default);

    Task<Supplier?> GetSupplierByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
