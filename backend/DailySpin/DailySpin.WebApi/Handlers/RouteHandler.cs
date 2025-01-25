using DailySpin.Application;
using DailySpin.DI;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace DailySpin.WebApi;

public class RouteHandler : IHandler
{
    private readonly Dictionary<string, Func<HttpListenerRequest, Task<object?>>> _routes;
    private readonly IScope _scope;

    public RouteHandler(Assembly controllersAssembly, IScope scope)
    {
        _scope = scope;
        _routes = controllersAssembly.GetTypes()
            .Where(type => typeof(IController).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .SelectMany(controller => controller.GetMethods()
                .Where(method => method.GetCustomAttribute<RouteAttribute>() is not null &&
                                 method.GetCustomAttribute<HttpMethodAttribute>() is not null)
                .Select(method => new
                {
                    Path = GetPath(controller, method),
                    Endpoint = GetEndpointMethod(controller, method)
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

    private Func<HttpListenerRequest, Task<object?>> GetEndpointMethod(Type controllerType, MethodInfo method)
    {
        return async (request) =>
        {
            var parameters = method.GetParameters();
            var parameterValues = new List<object?>();
            await Console.Out.WriteLineAsync("1212");
            foreach (var parameter in parameters)
            {
                if (parameter.GetCustomAttribute<FromBodyAttribute>() != null)
                {
                    using var reader = new StreamReader(request.InputStream);
                    var body = await reader.ReadToEndAsync();
                    var json = JsonSerializer.Deserialize(body, parameter.ParameterType);
                    parameterValues.Add(json);
                }
                else if (parameter.GetCustomAttribute<FromHeaderAttribute>() != null)
                {
                    var headerValue = request.Headers[parameter.Name];
                    var convertedValue = Convert.ChangeType(headerValue, parameter.ParameterType);
                    parameterValues.Add(convertedValue);
                }
                else if (parameter.GetCustomAttribute<FromQueryAttribute>() != null)
                {
                    var queryValue = request.QueryString[parameter.Name];
                    var convertedValue = Convert.ChangeType(queryValue, parameter.ParameterType);
                    parameterValues.Add(convertedValue);
                }
                else
                {
                    parameterValues.Add(null);
                }
            }

            var instance = _scope.Resolve(controllerType);
            return method.Invoke(instance, parameterValues.ToArray());
        };
    }



    public async Task HandleAsync(HttpListenerRequest request, HttpListenerResponse response)
    {
        var routeKey = $"{request.HttpMethod} {request.Url?.LocalPath}";

        if (!_routes.TryGetValue(routeKey, out var func))
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        response.StatusCode = (int)HttpStatusCode.OK;
        await WriteControllerResponseAsync(func(request), response);
    }

    private async Task WriteControllerResponseAsync(object? response, HttpListenerResponse listenerResponse)
    {
        if (response is byte[] buffer)
        {
            await listenerResponse.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        else if (response is Task task)
        {
            await task;

            var taskType = task.GetType();

            if (!taskType.IsGenericType)
                return;

            dynamic result = ((dynamic)task).Result;

            await WriteControllerResponseAsync(result, listenerResponse);
        }
        else
        {
            await WriteControllerResponseAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response)), listenerResponse);
        }
    }
}