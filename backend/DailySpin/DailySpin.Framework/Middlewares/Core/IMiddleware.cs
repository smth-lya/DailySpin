namespace DailySpin.Framework;

public interface IMiddleware
{
    Task InvokeAsync(HttpContext context, RequestDelegate next);
}
//Интерцептор 