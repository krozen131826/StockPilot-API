using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stockpilot.Identity;
using Stockpilot.Identity.Models;

namespace Stockpilot.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AuthenticationService _authService;

    public AuthController(IMediator mediator, AuthenticationService authService)
    {
        _mediator = mediator;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthServiceRequestModel request)
    {
        var response = await _authService.Authentication(request);

        return Ok(response);
    }
}
