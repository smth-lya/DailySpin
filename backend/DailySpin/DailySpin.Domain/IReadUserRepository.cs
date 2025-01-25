using Ardalis.Result;

namespace DailySpin.Domain;

public interface IUserReadRepository
{
    Task<Result<User>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<User>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
}