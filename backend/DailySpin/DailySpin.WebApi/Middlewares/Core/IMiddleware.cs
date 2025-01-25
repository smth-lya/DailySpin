namespace DailySpin.WebApi;

public interface IMiddleware
{
    Task InvokeAsync(HttpContext context, RequestDelegate next);
}
//Интерцептор 