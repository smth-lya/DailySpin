namespace DailySpin.WebApi;

internal class StaticFileMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var path = Path.Combine(context.Request.RawUrl?.Split('/')!);

        context.Response.ContentType = path?.Split('.')[^1] switch
        {
            "html" => "text/html",
            "css" => "text/css",
            "js" => "application/javascript",
            "png" => "image/png",
            "jpg" => "image/jpeg",
            "jpeg" => "image/jpeg",
            "gif" => "image/gif",
            "svg" => "image/svg+xml",
            "ico" => "image/x-icon",
            _ => "application/octet-stream",
        };

        path = $"wwwroot/{path}";

        if (File.Exists(path))
        {
            context.Response.StatusCode = 200;
            var file = await File.ReadAllBytesAsync(path);
            await context.Response.OutputStream.WriteAsync(file);
            return;
        }

        await next(context);
    }
}

