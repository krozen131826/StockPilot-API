using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockpilot.Application.Features.Product.Commands;
using Stockpilot.Application.Features.Product.Dtos;
using Stockpilot.Application.Features.Product.Queries;
using Stockpilot.Application.Features.PurchaseOrder.Commands;
using Stockpilot.Application.Features.PurchaseOrder.Dtos;
using Stockpilot.Application.Features.PurchaseOrder.Queries;
using Stockpilot.Application.Features.PurchaseOrderItem.Dto;

namespace Stockpilot.Api.Controllers;

[Route("api/purchaseOrder")]
[ApiController]
[Authorize]
public class PurchaseOrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseOrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddPurchaseOrder([FromBody] AddPurchaseOrderDto request)
    {

        await _mediator.Send(new AddPurchaseOrderCommand(request));

        return Ok();
    }



    //[HttpPut("update/{id}")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    //[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto request)
    //{
    //    await _mediator.Send(new UpdateProductCommand(id, request));

    //    return Ok();
    //}

    //[HttpPut("delete/{id}")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    //[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> DeleteProduct(Guid id)
    //{
    //    await _mediator.Send(new DeleteProductCommand(id));

    //    return Ok();
    //}

    [HttpGet("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<GetPurchaseOrderDto>>> ProductList()
    {
        var results = await _mediator.Send(new GetPurchaseOrderListQuery());

        return Ok(results);
    }


    [HttpGet("getById/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetPurchaseOrderDto>> GetPurchaseOrderById(Guid id)
    {
        var result = await _mediator.Send(new GetPurchaseOrderByIdQuery(id));

        return Ok(result);
    }

    [HttpPost("submit/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitPurchaseOrder(Guid id)
    {
        await _mediator.Send(new SubmitPurchaseOrderCommand(id));

        return Ok();
    }

    [HttpPost("receive/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReceivePurchaseOrder(Guid id)
    {
        await _mediator.Send(new ReceivePurchaseOrderCommand(id));

        return Ok();
    }
}
