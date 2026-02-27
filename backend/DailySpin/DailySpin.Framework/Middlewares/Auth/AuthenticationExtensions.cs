namespace DailySpin.Framework;

public static class AuthenticationExtensions
{
    public static IPipelineBuilder UseAuthentication(this IPipelineBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}
