﻿namespace DailySpin.WebApi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RouteAttribute : Attribute
{
    public string Path { get; }

    public RouteAttribute(string path)
    {
        Path = path;
    }
}
