using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace skill_tree.Common;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is UnauthorizedAccessException)
        {
            httpContext.Response.StatusCode = 401;
            await httpContext.Response.WriteAsync("User identification header is missing or invalid", cancellationToken: cancellationToken);
            return true;
        } else if (exception is InvalidOperationException)
        {
            httpContext.Response.StatusCode = 500; 
            await httpContext.Response.WriteAsync("An internal database or domain state error occurred.", cancellationToken: cancellationToken);
            return true;
        }

        return false;
    }
}