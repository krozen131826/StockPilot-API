using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockpilot.Application.Features.Category.Commands;
using Stockpilot.Application.Features.Category.Dtos;
using Stockpilot.Application.Features.Category.Queries;

namespace Stockpilot.Api.Controllers;

[Authorize]
[Route("api/category")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCategory([FromBody] AddCategoryDto request)
    {

        var categoryId = await _mediator.Send(new AddCategoryCommand(request));

        return Ok(categoryId);
    }

    

    [HttpPut("update/{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto request)
    {
        await _mediator.Send(new UpdateCategoryCommand(id, request));

        return Ok();
    }

    [HttpPut("delete/{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));

        return Ok();
    }

    [HttpGet("list")]
    [Authorize(Roles = "Administrator,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<CategoryListDto>>> CategoryList()
    {
       var results = await _mediator.Send(new GetCategoryListQuery());

        return Ok(results);
    }


    [HttpGet("getById/{id}")]
    [Authorize(Roles = "Administrator,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetCategoryByIdDto>> GetCategoryById(Guid id)
    {
        var result = await _mediator.Send(new GetCategroyByIdQuery(id));

        return Ok(result);
    }

}
