namespace DailySpin.Framework;

public static class EndpointRoutingExtensions
{
    public static IPipelineBuilder UseRouting(this IPipelineBuilder builder)
    {
        return builder.UseMiddleware<EndpointRoutingMiddleware>();
    }
}
