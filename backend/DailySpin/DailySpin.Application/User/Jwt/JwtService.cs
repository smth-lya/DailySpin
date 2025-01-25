using System.Security.Claims;
using Ardalis.Result;
using DailySpin.Domain;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DailySpin.Application;

public sealed class JwtService : IJwtService
{
    private readonly IJwtEncoder _encoder;
    private readonly IJwtRefreshTokenStorage _refreshTokenStorage;
    private readonly IUserReadRepository _userReadRepository;
    private readonly JwtOptions _options;

    public JwtService(IJwtEncoder encoder, IJwtRefreshTokenStorage refreshTokenStorage,
       IUserReadRepository userReadRepository, JwtOptions jwtOptions)
    {
        _encoder = encoder;
        _refreshTokenStorage = refreshTokenStorage;
        _userReadRepository = userReadRepository;
        _options = jwtOptions;
    }

    public void InvalidateRefreshToken(JwtToken refreshToken)
    {
        _refreshTokenStorage.Remove(refreshToken);
    }

    public JwtTokenPair Issue(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);

        _refreshTokenStorage.Store(refreshToken, _options.RefreshTokenExpirationTime);

        return new JwtTokenPair(accessToken, refreshToken);
    }

    public async Task<Result<JwtTokenPair>> RefreshAsync(JwtToken refreshToken, CancellationToken cancellationToken = default)
    {
        if (!_refreshTokenStorage.IsValid(refreshToken))
        {
            return Result.Error("Invalid refresh token. Maybe it has expired.");
        }

        _refreshTokenStorage.Remove(refreshToken);

        var claims = _encoder.DecodeToken(refreshToken).ToList();
        var userId = Guid.Parse(claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);

        long expirationtime = long.Parse(claims.Single(c => c.Type == JwtRegisteredClaimNames.Exp).Value);
        long currentTime = EpochTime.GetIntDate(DateTime.UtcNow);

        if (expirationtime < currentTime)
        {
            return Result.Error("Token expired.");
        }

        var user = await _userReadRepository.GetUserByIdAsync(userId, cancellationToken);
        if (!user.IsSuccess)
        {
            return Result.NotFound("User not found.");
        }

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken(user);

        _refreshTokenStorage.Store(newRefreshToken, _options.RefreshTokenExpirationTime);

        return new JwtTokenPair(newAccessToken, newRefreshToken);
    }

    private JwtToken GenerateAccessToken(User user)
    {
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
         // new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(ClaimTypes.Role, "User"),
            new(JwtTokenConstants.TOKEN_TYPE_CLAIM_NAME, JwtTokenConstants.ACCESS_TOKEN_TYPE)
        ];

        return _encoder.CreateToken(claims, _options.AccessTokenExpirationTime);
    }

    private JwtToken GenerateRefreshToken(User user)
    {
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtTokenConstants.TOKEN_TYPE_CLAIM_NAME, JwtTokenConstants.REFRESH_TOKEN_TYPE)
        ];

        return _encoder.CreateToken(claims, _options.RefreshTokenExpirationTime);
    }
}
