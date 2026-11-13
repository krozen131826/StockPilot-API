using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Stockpilot.Api.Controllers;

[Route("api/purchaseOrderItem")]
[ApiController]
[Authorize]
public class PurchaseOrderItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseOrderItemController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
