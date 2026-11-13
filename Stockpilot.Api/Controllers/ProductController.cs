using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockpilot.Application.Features.Product.Commands;
using Stockpilot.Application.Features.Product.Dtos;
using Stockpilot.Application.Features.Product.Queries;
using Stockpilot.Application.Features.StockMovement.Command;
using Stockpilot.Application.Features.StockMovement.Dtos;
using Stockpilot.Application.Features.Supplier.Commands;
using Stockpilot.Application.Features.Supplier.Dtos;
using Stockpilot.Application.Features.Supplier.Queries;

namespace Stockpilot.Api.Controllers;

[Authorize]
[Route("api/product")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProduct([FromBody] AddProductDto request)
    {

        var productId = await _mediator.Send(new AddProductCommand(request));

        return Ok(productId);
    }



    [HttpPut("update/{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto request)
    {
        await _mediator.Send(new UpdateProductCommand(id, request));

        return Ok();
    }

    [HttpPut("delete/{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _mediator.Send(new DeleteProductCommand(id));

        return Ok();
    }

    [HttpGet("list")]
    [Authorize(Roles = "Administrator,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<GetProductListDto>>> ProductList()
    {
        var results = await _mediator.Send(new GetProductListQuery());

        return Ok(results);
    }


    [HttpGet("getById/{id}")]
    [Authorize(Roles = "Administrator,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetProductByIdDto>> GetProductById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));

        return Ok(result);
    }


}
