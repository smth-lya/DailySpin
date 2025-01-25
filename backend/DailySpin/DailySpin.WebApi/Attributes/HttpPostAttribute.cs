using System.Diagnostics.CodeAnalysis;

namespace DailySpin.WebApi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute() : base("POST")
    {
    }
}