using Ardalis.Result;

namespace DailySpin.Domain;

public interface IJwtService
{
    JwtTokenPair Issue(User user);
    Task<Result<JwtTokenPair>> RefreshAsync(JwtToken refreshToken, CancellationToken cancellationToken = default);
    void InvalidateRefreshToken(JwtToken refreshToken);
}
