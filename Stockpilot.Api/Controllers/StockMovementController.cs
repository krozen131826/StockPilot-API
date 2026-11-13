using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockpilot.Application.Features.StockMovement.Command;
using Stockpilot.Application.Features.StockMovement.Dtos;
using Stockpilot.Application.Features.StockMovement.Query;

namespace Stockpilot.Api.Controllers;

[Route("api/stockMovement")]
[ApiController]
[Authorize]
public class StockMovementController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockMovementController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("productId/{productId}/stock-in")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddStockMovementProduct(Guid productId, [FromBody] AddStockMovementDto request)
    {

        await _mediator.Send(new AddStockMovementCommand(productId, request));

        return Ok();
    }

    [HttpPost("productId/{productId}/adjustment")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AdjustmentStockMovementProduct(Guid productId, [FromBody] AdjustStockMovementDto request)
    {

        await _mediator.Send(new AdjustmentStockMovementCommand(productId, request));

        return Ok();
    }

    [HttpGet("list")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<GetStockMovementListDto>>> GetStockMovementList()
    {
        var stockMovements = await _mediator.Send(new GetStockMovementListQuery());

        return Ok(stockMovements);
    }
}
