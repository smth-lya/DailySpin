namespace DailySpin.Domain;

public sealed record JwtTokenPair(JwtToken AccessToken, JwtToken RefreshToken);
