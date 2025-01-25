using DailySpin.Application;
using DailySpin.Domain;

namespace DailySpin.WebApi;

public sealed class ValidateEndpoint : IController
{
    private readonly IJwtEncoder _jwtEncoder;

    public ValidateEndpoint(IJwtEncoder jwtEncoder)
    {
        _jwtEncoder = jwtEncoder;
    }

    [HttpPost]
    [Route("/api/auth/validate")]
    public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
    {
        var jwtToken = new JwtToken(request.AccessToken);

        try
        {
            var claims = _jwtEncoder.ValidateToken(jwtToken);
        }
        catch (Exception)
        {
            return this.BadRequest("Invalid token.");
        }

        return this.Ok(new ValidateTokenResponse(true));
    }

    public sealed record ValidateTokenRequest(string AccessToken);

    public sealed record ValidateTokenResponse(bool IsValid);
}
