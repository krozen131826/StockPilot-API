using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Category.Dtos;

namespace Stockpilot.Application.Features.Category.Commands;

public record UpdateCategoryCommand(Guid id, UpdateCategoryDto request) : IRequest<Unit>;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.request.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.request.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}

internal sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {

        var validator = new UpdateCategoryCommandValidator();

        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new ValidationExceptions(validationResult);
        }
        var category = await _categoryRepository.GetCategoryByIdAsync(request.id, cancellationToken);

        if (category == null)
        {
            throw new NotFoundExceptions($"Category with ID '{request.id}' not found.");
        }
        // Check if the new name already exists for another category
        if (await _categoryRepository.ExistsByNameAsync(request.request.Name, cancellationToken) &&
            !string.Equals(category.Name, request.request.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestExceptions($"A category with the name '{request.request.Name}' already exists.");
        }
        // Update the category properties
        category.Name = request.request.Name;
        category.Description = request.request.Description;

        // Save changes to the repository
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}