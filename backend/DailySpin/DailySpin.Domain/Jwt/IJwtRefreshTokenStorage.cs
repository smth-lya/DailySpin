namespace DailySpin.Domain;

public interface IJwtRefreshTokenStorage
{
    bool IsValid(JwtToken refreshToken);
    void Remove(JwtToken refreshToken);
    void Store(JwtToken refreshToken, TimeSpan expirationTime);
}