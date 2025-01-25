namespace DailySpin.WebApi;

public static class AuthorizationExtensions
{
    public static IPipelineBuilder UseAuthorization(this IPipelineBuilder builder)
    {
        return builder.UseMiddleware<AuthorizationMiddleware>();
    }
}
