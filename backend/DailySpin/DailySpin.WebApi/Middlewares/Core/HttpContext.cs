using System.Net;
using System.Security.Claims;

namespace DailySpin.WebApi;

public class HttpContext
{
    /// <summary>
    /// Получает коллекцию функций HTTP, предоставляемых сервером и промежуточным программным обеспечением, доступным по этому запросу.
    /// </summary>
    public IFeatureCollection Features { get; } = new FeatureCollection(10);
    public required HttpListenerRequest Request { get; init; }
    public required HttpListenerResponse Response { get; init; }

    public ClaimsPrincipal? User;
}