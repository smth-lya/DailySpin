using DailySpin.Application;
using DailySpin.Domain;
using static DailySpin.WebApi.SignInEndpoint;

namespace DailySpin.WebApi;

public sealed class RefreshEndpoint : IController
{
    private readonly IJwtService _jwtService;

    public RefreshEndpoint(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost]
    [Route("/api/auth/refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, HttpContext context)
    {
        var jwtToken = new JwtToken(request.RefreshToken);

        var result = await _jwtService.RefreshAsync(jwtToken);

        if (!result.IsSuccess)
        {
            return this.BadRequest(string.Join("; ", result.Errors));
        }
        
        var refreshCookie = $"{JwtTokenConstants.REFRESH_TOKEN_TYPE}={result.Value.RefreshToken}; " +
                        $"HttpOnly; Secure; SameSite=Strict; Expires={DateTime.UtcNow.AddDays(7):R}";

        context.Response.Headers.Add("Set-Cookie", refreshCookie);

        var response = new SignInResponse(result.Value.AccessToken.Token, result.Value.RefreshToken.Token);
        return this.Ok(response);
     
    }

    public record RefreshRequest(string RefreshToken);

    public sealed record RefreshResponse(string AccessToken, string RefreshToken);
}
