using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockpilot.Application.Features.Category.Commands;
using Stockpilot.Application.Features.Category.Dtos;

namespace Stockpilot.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    public async Task<IActionResult> CreateCategory([FromBody] AddCategoryDto request)
    {

        var categoryId = await _mediator.Send(new CategoryCommand(request));

        return Ok(categoryId);
    }
}
