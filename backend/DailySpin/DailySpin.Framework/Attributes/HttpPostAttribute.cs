namespace DailySpin.Framework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute() : base("POST")
    {
    }
}