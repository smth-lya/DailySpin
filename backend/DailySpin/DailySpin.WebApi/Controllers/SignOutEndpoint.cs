using DailySpin.Application;
using DailySpin.Domain;

namespace DailySpin.WebApi;

public sealed class SignOutEndpoint : IController
{
    private readonly IUserService _userService;

    public SignOutEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [Route("/api/auth/signout")]
    public async Task<IActionResult> SignOut([FromBody] SignOutRequest request)
    {
        var jwtToken = new JwtToken(request.RefreshToken);

        await _userService.SignOutAsync(jwtToken);

        return this.Ok();
    }

    public record SignOutRequest(string RefreshToken);
}
