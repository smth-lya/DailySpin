namespace DailySpin.WebApi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpMethodAttribute : Attribute
{
    public string Method { get; }

    public HttpMethodAttribute(string method)
    {
        Method = method.ToUpper();
    }
}
