using Ardalis.Result;
using DailySpin.Domain;

namespace DailySpin.Application;

public interface IUserService
{
    Task<Result<User>> SignUpAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<JwtTokenPair>> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result> SignOutAsync(JwtToken refreshToken, CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _repository;
    private readonly IJwtService _jwtService;

    public UserService(IPasswordHasher passwordHasher, IUserRepository repository, IJwtService jwtService)
    {
        _passwordHasher = passwordHasher;
        _repository = repository;
        _jwtService = jwtService;
    }

    public async Task<Result<User>> SignUpAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var passwordHash = _passwordHasher.Hash(password);

        var user = new User(email, passwordHash);

        var result = await _repository.AddAsync(user, cancellationToken);

        if (!result.IsSuccess)
            return result;

        return Result.Success(user);
    }

    public async Task<Result<JwtTokenPair>> SignInAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetUserByUsernameAsync(username, cancellationToken);

        if (!result.IsSuccess)
            return result.IsNotFound() 
                ? Result.NotFound()
                : Result.Error("[Internal error] in the [User Repository] during [sign in]");

        var user = result.Value;

        var isVerified = _passwordHasher.Verify(password, user.PasswordHash);

        if (!isVerified)
            return Result.Invalid(new ValidationError("Invalid password"));

        var tokenPair = _jwtService.Issue(user);

        return Result.Success(tokenPair);
    }

    public Task<Result> SignOutAsync(JwtToken refreshToken, CancellationToken cancellationToken = default)
    {
        _jwtService.InvalidateRefreshToken(refreshToken);

        return Task.FromResult(Result.Success());
    }
}
