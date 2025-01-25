namespace DailySpin.WebApi;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class FromBodyAttribute : Attribute { }