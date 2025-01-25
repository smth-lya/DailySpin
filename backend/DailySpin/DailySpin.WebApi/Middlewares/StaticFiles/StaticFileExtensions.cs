namespace DailySpin.WebApi;

public static class StaticFileExtensions
{
    public static IPipelineBuilder UseStaticFile(this IPipelineBuilder builder)
    {
        return builder.UseMiddleware<StaticFileMiddleware>();
    }
}