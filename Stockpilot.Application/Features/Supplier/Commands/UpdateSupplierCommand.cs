using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Supplier.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Supplier.Commands;

public record UpdateSupplierCommand(Guid id,UpdateSupplierDto request) : IRequest<Unit>;

internal sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.request.Name)
           .NotEmpty().WithMessage("Supplier name is required.")
           .MaximumLength(100).WithMessage("Supplier name cannot exceed 100 characters.");

        RuleFor(x => x.request.ContactPerson)
            .MaximumLength(100).WithMessage("Contact person name cannot exceed 100 characters.");

        RuleFor(x => x.request.Address)
            .MaximumLength(100).WithMessage("Address cannot exceed 100 characters.");

        RuleFor(x => x.request.Email)
            .EmailAddress().WithMessage("Invalid email address format.")
            .When(x => !string.IsNullOrEmpty(x.request.Email));

        RuleFor(x => x.request.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.")
            .When(x => !string.IsNullOrEmpty(x.request.Phone));
    }
}

internal sealed class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Unit>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateSupplierCommandHandler(ISupplierRepository supplierRepository, IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Unit> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSupplierCommandValidator();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new ValidationExceptions(validationResult);
        }

        var supplier = await _supplierRepository.GetSupplierByIdAsync(request.id, cancellationToken);
        if (supplier == null)
        {
            throw new NotFoundExceptions($"Supplier with ID '{request.id}' not found.");
        }

        // Check if a supplier with the same name already exists
        if (await _supplierRepository.IsSupplierExistsAsync(request.request.Name, cancellationToken) &&
            !string.Equals(supplier.Name, request.request.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestExceptions($"A supplier with the name '{request.request.Name}' already exists.");
        }

        // Update the supplier properties
        supplier.Name = request.request.Name;
        supplier.ContactPerson = request.request.ContactPerson;
        supplier.Email = request.request.Email;
        supplier.Phone = request.request.Phone;
        supplier.Address = request.request.Address;

        // Save changes to the repository
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}