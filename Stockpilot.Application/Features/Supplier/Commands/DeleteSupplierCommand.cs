using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;

namespace Stockpilot.Application.Features.Supplier.Commands;

public record DeleteSupplierCommand(Guid id) : IRequest<bool>;

internal sealed class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, bool>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    public DeleteSupplierCommandHandler(ISupplierRepository supplierRepository, IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<bool> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetSupplierByIdAsync(request.id, cancellationToken);

        if (supplier is null)
        {
            throw new NotFoundExceptions($"Supplier with ID {request.id} not found");
        }

        supplier.IsActive = false;
        supplier.IsDeleted = true;
        supplier.DeletedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
