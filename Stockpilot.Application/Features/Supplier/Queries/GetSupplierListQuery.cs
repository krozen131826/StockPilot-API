using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Supplier.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Supplier.Queries;

public record GetSupplierListQuery() : IRequest<IReadOnlyList<GetSupplierListDto>>;

internal sealed class GetSupplierListQueryHandler : IRequestHandler<GetSupplierListQuery, IReadOnlyList<GetSupplierListDto>>
{
    private readonly ISupplierRepository _supplierRepository;
    public GetSupplierListQueryHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }
    public async Task<IReadOnlyList<GetSupplierListDto>> Handle(GetSupplierListQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await _supplierRepository.GetSupplierListAsync(cancellationToken);

        if(suppliers == null || !suppliers.Any())
        {
            throw new NotFoundExceptions("No Suppliers Available");
        }

        return suppliers.Select(s => new GetSupplierListDto
        {
            Id = s.Id,
            Name = s.Name,
            ContactPerson = s.ContactPerson,
            Email = s.Email,
            Phone = s.Phone,
            Address = s.Address
        }).ToList();
    }
}
