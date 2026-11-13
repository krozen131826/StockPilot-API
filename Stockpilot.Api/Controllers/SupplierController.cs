using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockpilot.Application.Features.Supplier.Commands;
using Stockpilot.Application.Features.Supplier.Dtos;
using Stockpilot.Application.Features.Supplier.Queries;

namespace Stockpilot.Api.Controllers;

[Authorize]
[Route("api/supplier")]
[ApiController]
public class SupplierController : ControllerBase
{
    private readonly IMediator _mediator;

    public SupplierController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSupplier([FromBody] AddSupplierDto request)
    {

        var supplierId = await _mediator.Send(new AddSupplierCommand(request));

        return Ok(supplierId);
    }



    [HttpPut("update/{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierDto request)
    {
        await _mediator.Send(new UpdateSupplierCommand(id, request));

        return Ok();
    }

    [HttpPut("delete/{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSupplier(Guid id)
    {
        await _mediator.Send(new DeleteSupplierCommand(id));

        return Ok();
    }

    [HttpGet("list")]
    [Authorize(Roles = "Administrator,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<GetSupplierListDto>>> SupplierList()
    {
        var results = await _mediator.Send(new GetSupplierListQuery());

        return Ok(results);
    }


    [HttpGet("getById/{id}")]
    [Authorize(Roles = "Administrator,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetSupplierByIdDto>> GetSupplierById(Guid id)
    {
        var result = await _mediator.Send(new GetSupplierByIdQuery(id));

        return Ok(result);
    }

}
