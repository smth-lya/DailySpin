namespace DailySpin.WebApi;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class AuthorizeAttribute : Attribute
{
    public string RequiredRole {  get; init; }

    public AuthorizeAttribute(string requiredRole)
    {
        RequiredRole = requiredRole;
    }
}
