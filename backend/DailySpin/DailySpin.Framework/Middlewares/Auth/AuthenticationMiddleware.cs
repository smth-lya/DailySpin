using DailySpin.Domain;

namespace DailySpin.Framework;

public class AuthenticationMiddleware : IMiddleware
{
    private IJwtEncoder _jwtEncoder;

    public AuthenticationMiddleware(IJwtEncoder jwtEncoder)
    {
        _jwtEncoder = jwtEncoder;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint == null)
            return;

        if (endpoint.Metadata.GetMetadata<AuthorizeAttribute>() is null)
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"];

        if (authHeader is null || !authHeader.StartsWith("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            var claims = _jwtEncoder.ValidateToken(new(token));
            context.User = claims;

            await next(context);
        }
        catch (Exception)
        {
            context.Response.StatusCode = 401;
        }
    }
}
