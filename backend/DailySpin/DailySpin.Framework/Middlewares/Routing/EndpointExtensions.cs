namespace DailySpin.Framework;

public static class EndpointExtensions
{
    public static IPipelineBuilder UseEndpoints(this IPipelineBuilder builder)
    {
        return builder.UseMiddleware<EndpointMiddleware>();
    }
}