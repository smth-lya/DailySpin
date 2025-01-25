using DailySpin.WebApi;

namespace DailySpin.WebApi;

public class AuthorizationMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint == null)
            return;

        var authMetadata = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();

        if (authMetadata == null)
        {
            await next(context);
            return;
        }

        var user = context.User;

        if (user == null)
            return;

        if (!user.IsInRole(authMetadata.RequiredRole))
        {
            context.Response.StatusCode = 403; 
            return;
        }

        context.Response.StatusCode = 200;
        await next(context);
    }
}
