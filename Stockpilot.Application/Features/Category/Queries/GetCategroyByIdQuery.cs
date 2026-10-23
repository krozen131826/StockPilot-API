using FluentValidation;
using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Exceptions;
using Stockpilot.Application.Features.Category.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Features.Category.Queries;

public record GetCategroyByIdQuery(Guid Id) : IRequest<GetCategoryByIdDto>;

internal sealed class GetCategroyByIdQueryValidator : AbstractValidator<GetCategroyByIdQuery>
{
    public GetCategroyByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
    }
}

internal sealed class GetCategroyByIdQueryHandler : IRequestHandler<GetCategroyByIdQuery, GetCategoryByIdDto>
{
    private readonly ICategoryRepository _categoryRepository;
    public GetCategroyByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    public async Task<GetCategoryByIdDto> Handle(GetCategroyByIdQuery request, CancellationToken cancellationToken)
    {

        var validator = new GetCategroyByIdQueryValidator();
        
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        { 
            throw new ValidationExceptions(validationResult);
        }

        var category = await _categoryRepository.GetCategoryByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundExceptions($"Category with ID {request.Id} not found");
        }

        return new GetCategoryByIdDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

    }
}
