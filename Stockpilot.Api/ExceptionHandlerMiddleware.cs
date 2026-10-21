using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Stockpilot.Application.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Stockpilot.Api;

public class ExceptionHandlerMiddleware : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {

        var problemDetails = exception switch
        {
            ValidationExceptions ve => new ProblemDetails
            {
                Title = "ValidationErrors",
                Detail = "Error Validating Request",
                Type = "Validation Errors",
                Status = StatusCodes.Status400BadRequest,
                Extensions = new Dictionary<string, object?>
                {
                    {"errors", ve.ValidationErrors }
                }
            },
            BadRequestExceptions br => new ProblemDetails
            {
                Title = "BadRequestException",
                Detail = br.Message,
                Type = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
            },
            NotFoundExceptions nf => new ProblemDetails
            {
                Title = "NotFoundException",
                Detail = nf.Message,
                Type = "Not Found",
                Status = StatusCodes.Status404NotFound,
            },
            UnAuthorizedExceptions ua => new ProblemDetails
            {
                Title = "UnauthorizedException",
                Detail = ua.Message,
                Type = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
            },
            _ => new ProblemDetails
            {
                Title = "ServerError",
                Type = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
            },
        };

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
