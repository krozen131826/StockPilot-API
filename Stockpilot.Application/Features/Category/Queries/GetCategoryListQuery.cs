using MediatR;
using Stockpilot.Application.Contracts;
using Stockpilot.Application.Features.Category.Dtos;

namespace Stockpilot.Application.Features.Category.Queries;

public record GetCategoryListQuery() : IRequest<List<CategoryListDto>>;


internal sealed class GetCategoryListQueryHandler : IRequestHandler<GetCategoryListQuery, List<CategoryListDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    public GetCategoryListQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    public async Task<List<CategoryListDto>> Handle(GetCategoryListQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetCategoryListAsync(cancellationToken);

        return categories.Select(c => new CategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).ToList();
    }
}