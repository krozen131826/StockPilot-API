using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Supplier.Dtos;
namespace Stockpilot.Application.Features.Supplier.Commands;

public record AddSupplierCommand(AddSupplierDto request) : IRequest<Guid>;

internal sealed class AddSupplierCommandValidator : AbstractValidator<AddSupplierCommand>
{
    public AddSupplierCommandValidator()
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

internal sealed class AddSupplierCommandHandler : IRequestHandler<AddSupplierCommand, Guid>
{
    private readonly ISuppllierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    public AddSupplierCommandHandler(ISuppllierRepository supplierRepository, IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Guid> Handle(AddSupplierCommand request, CancellationToken cancellationToken)
    {
        var validator = new AddSupplierCommandValidator();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new ValidationExceptions(validationResult);
        }

        // Check if a supplier with the same name already exists
        if (await _supplierRepository.IsSupplierExistsAsync(request.request.Name, cancellationToken))
        {
            throw new BadRequestExceptions($"A supplier with the name '{request.request.Name}' already exists.");
        }
        var supplier = new Domain.Entities.Supplier
        {
            Id = Guid.NewGuid(),
            Name = request.request.Name,
            ContactPerson = request.request.ContactPerson,
            Email = request.request.Email,
            Phone = request.request.Phone,
            Address = request.request.Address
        };

         _supplierRepository.AddSupplier(supplier, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return supplier.Id;
    }
}

