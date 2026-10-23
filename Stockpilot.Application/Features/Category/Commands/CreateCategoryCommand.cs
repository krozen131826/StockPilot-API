using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
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

        var validator = new CategoryCommandValidator();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count() > 0)
        {
            throw new ValidationExceptions(validationResult);
        }

        // Check if the category name already exists
        var existingCategory = await _categoryRepository.ExistsByNameAsync(request.request.Name, cancellationToken);

        if (existingCategory)
        {
            throw new BadRequestExceptions("A category with the specified name already exists.");
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
        _categoryRepository.AddCategory(category, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Return the ID of the newly created category
        return category.Id;
    }
}