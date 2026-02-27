namespace DailySpin.Framework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute() : base("GET")
    {
    }
}