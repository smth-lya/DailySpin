using DailySpin.Application;
using DailySpin.WebApi;
using System.Reflection;
using System.Text.Json;
using System.Text;
using Ardalis.Result;

public class EndpointRoutingMiddleware : IMiddleware
{
    private readonly Dictionary<string, Endpoint> _routes;
    private readonly IControllersFactory _controllerFactory;

    public EndpointRoutingMiddleware(Assembly controllersAssembly, IControllersFactory controllerFactory)
    {
        _controllerFactory = controllerFactory;

        _routes = controllersAssembly.GetTypes()
            .Where(type => typeof(IController).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .SelectMany(controller => controller.GetMethods()
                .Where(method => method.GetCustomAttribute<RouteAttribute>() is not null &&
                                 method.GetCustomAttribute<HttpMethodAttribute>() is not null)
                .Select(method => new
                {
                    Path = GetPath(controller, method),
                    Endpoint = GetEndpoint(controller, method)
                }))
            .ToDictionary(
                key => key.Path,
                value => value.Endpoint
            );
    }

    private string GetPath(Type controller, MethodInfo method)
    {
        var routeAttribute = method.GetCustomAttribute<RouteAttribute>();
        var httpMethodAttribute = method.GetCustomAttribute<HttpMethodAttribute>();

        if (routeAttribute == null || httpMethodAttribute == null)
            throw new InvalidOperationException("Method must have both Route and HttpMethod attributes.");

        return $"{httpMethodAttribute.Method} {routeAttribute.Path}";
    }
    private Endpoint GetEndpoint(Type controller, MethodInfo method)
    {
        var handler = GetEndpointMethod(controller, method);
        var metadata = method.GetCustomAttributes();
        var name = method.GetCustomAttribute<RouteAttribute>()?.Path;

        return new(handler, new(metadata), name);
    }
    private RequestDelegate GetEndpointMethod(Type controllerType, MethodInfo method)
    {
        return async (context) =>
        {
            var parameterValues = await GetParameterValues(method, context);
            var instance = _controllerFactory.CreateController(controllerType);
            var result = method.Invoke(instance, parameterValues.ToArray());

            context.Response.StatusCode = 200;
            context.Response.ContentType = method.GetCustomAttribute<ContentTypeAttribute>()?.ContentType ?? HttpContentType.Json;

            await WriteResponseAsync(result, context);
        };
    }

    private async Task<List<object?>> GetParameterValues(MethodInfo method, HttpContext context)
    {
        var parameters = method.GetParameters();
        var parameterValues = new List<object?>();

        foreach (var parameter in parameters)
        {
            object? value = parameter.ParameterType switch
            {
                var p when p == typeof(HttpContext) => context,
                _ when parameter.GetCustomAttribute<FromBodyAttribute>() != null => await ReadBodyAsync(parameter, context),
                _ when parameter.GetCustomAttribute<FromHeaderAttribute>() != null => GetFromHeader(parameter, context),
                _ when parameter.GetCustomAttribute<FromQueryAttribute>() != null => GetFromQuery(parameter, context),
                _ => null
            };
            parameterValues.Add(value);
        }

        return parameterValues;
    }

    private async Task<object?> ReadBodyAsync(ParameterInfo parameter, HttpContext context)
    {
        using var reader = new StreamReader(context.Request.InputStream);
        var body = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize(body, parameter.ParameterType, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
    }

    private object? GetFromHeader(ParameterInfo parameter, HttpContext context)
    {
        var headerValue = context.Request.Headers[parameter.Name!];
        return Convert.ChangeType(headerValue, parameter.ParameterType);
    }

    private object? GetFromQuery(ParameterInfo parameter, HttpContext context)
    {
        var queryValue = context.Request.QueryString[parameter.Name!];
        return Convert.ChangeType(queryValue, parameter.ParameterType);
    }


    async static Task WriteResponseAsync(object? response, HttpContext context)
    {
        if (response is IActionResult actionResult)
        {
            await actionResult.ExecuteAsync(context);
        }
        else if (response is byte[] buffer)
        {
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        else if (response is Task task)
        {
            await task;

            if (task.GetType().IsGenericType)
            {
                await WriteResponseAsync(((dynamic)task).Result, context);
            }
            else
                return;
        }
        else if (response is null)
        {
            context.Response.StatusCode = 204;
        }
        else
        {
            await WriteResponseAsync(
                       JsonSerializer.SerializeToUtf8Bytes(response, new JsonSerializerOptions()
                       {
                           PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                       }), context);
        }
    }


    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var routeKey = $"{context.Request.HttpMethod} {context.Request.RawUrl}";

        if (_routes.TryGetValue(routeKey, out var endpoint))
        {
            context.SetEndpoint(endpoint);
            await next(context);
        }
        else
        {
            context.Response.StatusCode = 404;            
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Route not found"));
        }
    }
}

