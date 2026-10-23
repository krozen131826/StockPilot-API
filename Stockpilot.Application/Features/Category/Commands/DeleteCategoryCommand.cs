using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Category.Commands;

public record DeleteCategoryCommand(Guid id) : IRequest<bool>;

internal sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetCategoryByIdAsync(request.id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundExceptions($"Category with ID {request.id} not found");
        }

        category.IsActive = false;
        category.IsDeleted = true;
        category.DeletedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }


    
}