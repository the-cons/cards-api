using Cards.Api.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cards.Api.Errors;

public class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            CollectionFullException => (StatusCodes.Status409Conflict, "Collection is full"),
            _ => (0, null)
        };

        if (status == 0) return false;

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = exception.Message
                }
        });
    }
}