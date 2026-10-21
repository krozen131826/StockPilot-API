using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Features.Category.Dtos;

namespace Stockpilot.Application.Features.Category.Commands;

public sealed record CategoryCommand(AddCategoryDto request) : IRequest<Guid>;


internal sealed class  CategoryCommandValidator : AbstractValidator<CategoryCommand>
{
    public CategoryCommandValidator()
    {
        RuleFor(x => x.request.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.request.Description)
            .MaximumLength(500).WithMessage("Category description must not exceed 500 characters.");
    }
}

internal sealed class CategoryCommandHandler : IRequestHandler<CategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    public CategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Guid> Handle(CategoryCommand request, CancellationToken cancellationToken)
    {
        // Check if the category name already exists
        if (await _categoryRepository.ExistsByNameAsync(request.request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"A category with the name '{request.request.Name}' already exists.");
        }
        // Create a new category entity
        var category = new Domain.Entities.Category
        {
            Id = Guid.NewGuid(),
            Name = request.request.Name,
            Description = request.request.Description,
            IsActive = true
        };

        // Add the new category to the repository
        await _categoryRepository.AddAsync(category, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return the ID of the newly created category
        return category.Id;
    }
}