namespace DailySpin.WebApi;

public class EndpointMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint == null)
            return;

        if (endpoint.RequestDelegate == null)
            return;

        await endpoint.RequestDelegate.Invoke(context);
    }
}
