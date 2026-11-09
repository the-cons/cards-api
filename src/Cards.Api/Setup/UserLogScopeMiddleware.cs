namespace Cards.Api.Setup;

public class UserLogScopeMiddleware(RequestDelegate next, ILogger<UserLogScopeMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["User"] = context.User.Identity?.Name ?? "anonymous"
               }))
        {
            await next(context);
        }
    }
}