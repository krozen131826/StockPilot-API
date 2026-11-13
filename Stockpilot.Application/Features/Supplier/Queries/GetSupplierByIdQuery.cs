using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Supplier.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Supplier.Queries;

public record GetSupplierByIdQuery(Guid id) : IRequest<GetSupplierByIdDto>;

internal sealed class GetSupplierByIdQueryValidator : AbstractValidator<GetSupplierByIdQuery>
{
    public GetSupplierByIdQueryValidator()
    {
        RuleFor(x => x.id).NotEmpty().WithMessage("Supplier ID is required");
    }
}

internal sealed class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, GetSupplierByIdDto>
{
    private readonly ISupplierRepository _supplierRepository;
    public GetSupplierByIdQueryHandler(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }
    public async Task<GetSupplierByIdDto> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetSupplierByIdQueryValidator();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new ValidationExceptions(validationResult);
        }

        var supplier = await _supplierRepository.GetSupplierByIdAsync(request.id, cancellationToken);

        if (supplier is null)
        {
            throw new NotFoundExceptions($"Supplier with ID {request.id} not found");
        }

        return new GetSupplierByIdDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address
        };
    }
}
