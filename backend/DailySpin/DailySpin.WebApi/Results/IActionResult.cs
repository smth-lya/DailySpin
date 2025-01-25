using System.Net;
using System.Text.Json;
using System.Text;
using DailySpin.Application;

namespace DailySpin.WebApi;

public interface IActionResult
{
    Task ExecuteAsync(HttpContext context);
}

public class OkResult : IActionResult
{
    private readonly object? _value;

    public OkResult(object? value)
    {
        _value = value;
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json";
        var jsonResponse = JsonSerializer.Serialize(_value, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(jsonResponse), 0, jsonResponse.Length);
    }
}
public class ContentResult : IActionResult
{
    private readonly string _content;
    private readonly string _contentType;
    private readonly int _statusCode;

    public ContentResult(string content, string contentType = "text/plain", int statusCode = 200)
    {
        _content = content;
        _contentType = contentType;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = _statusCode;
        context.Response.ContentType = _contentType;
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(_content), 0, _content.Length);
    }
}
public class RedirectResult : IActionResult
{
    private readonly string _url;

    public RedirectResult(string url)
    {
        _url = url;
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = 302;  // Статус код для редиректа
        context.Response.Redirect(_url);
        await Task.CompletedTask;
    }
}
public class BadRequestResult : IActionResult
{
    private string? _errors;

    public BadRequestResult()
    { }
    public BadRequestResult(string? errors)
    {
        _errors = errors;
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = 400;
        context.Response.StatusDescription = "Bad Request";
        context.Response.ContentType = "text/plain";

        var message = _errors ?? "Bad Request: The server could not understand the request";

        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(message), 0, message.Length);
    }
}
public class UnauthorizedResult : IActionResult
{
    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = 401;
        context.Response.StatusDescription = "Unauthorized";
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Unauthorized: Access is denied due to invalid credentials"), 0, 66);
    }
}
public class NotFoundResult : IActionResult
{
    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = 404;
        context.Response.StatusDescription = "Not Found";
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Not Found: The resource you requested could not be found"), 0, 69);
    }
}
public class InternalServerErrorResult : IActionResult
{
    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = 500;
        context.Response.StatusDescription = "Internal Server Error";
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Internal server error occurred"), 0, 26);
    }
}

public class ViewResult : IActionResult
{
    private readonly string _viewName;
    private readonly Dictionary<string, object> _model;

    public ViewResult(string viewName, Dictionary<string, object>? model = null)
    {
        _viewName = viewName;
        _model = model ?? new();
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        var viewPath = Path.Combine(Directory.GetCurrentDirectory(), "Views", _viewName + ".html");

        if (!File.Exists(viewPath))
        {
            context.Response.StatusCode = 404;
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("View not found"), 0, 15);
            return;
        }

        var viewContent = await File.ReadAllTextAsync(viewPath);

        foreach (var key in _model.Keys)
        {
            var placeholder = $"{{{{ {key} }}}}";
            viewContent = viewContent.Replace(placeholder, _model[key]?.ToString() ?? string.Empty);
        }

        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/html";
        await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(viewContent), 0, viewContent.Length);
    }
}

public static class ActionResultExtensions
{
    public static IActionResult Ok(this IController controller, object? value = null)
        => new OkResult(value);

    public static IActionResult Content(this IController controller, string content, string contentType = "text/plain", int statusCode = 200)
        => new ContentResult(content, contentType, statusCode);

    public static IActionResult BadRequest(this IController controller)
        => new BadRequestResult();

    public static IActionResult BadRequest(this IController controller, string? errors)
        => new BadRequestResult(errors);

    public static IActionResult NotFound(this IController controller)
        => new NotFoundResult();

    public static IActionResult View(this IController controller, string viewName, Dictionary<string, object>? model = null)
        => new ViewResult(viewName, model);

    public static IActionResult Redirect(this IController controller, string url)
        => new RedirectResult(url);

    public static IActionResult Unauthorized(this IController controller)
        => new UnauthorizedResult();

    public static IActionResult InternalServerError(this IController controller)
        => new InternalServerErrorResult();
}
