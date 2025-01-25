using System.Security.Claims;

namespace DailySpin.Domain;

public interface IJwtEncoder
{
    JwtToken CreateToken(IEnumerable<Claim> userClaims, TimeSpan expirationTime);
    IEnumerable<Claim> DecodeToken(JwtToken token);
    ClaimsPrincipal ValidateToken(JwtToken token);
}
