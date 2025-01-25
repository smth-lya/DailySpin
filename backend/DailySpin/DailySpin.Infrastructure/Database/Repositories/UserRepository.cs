using Ardalis.Result;
using DailySpin.Domain;
using DailySpin.ORM;

namespace DailySpin.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<User>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user is null ? Result<User>.NotFound() : Result.Success(user);
    }
    public async Task<Result<User>> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        return user is null ? Result<User>.NotFound() : Result.Success(user);
    }

    public async Task<Result> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var loadedUser = await GetUserByIdAsync(user.Id);

        if (!loadedUser.IsSuccess && loadedUser.Value is not null)
        {
            return Result.Error();
        }

        _dbContext.Add(user);
        
        var written = await _dbContext.SaveChangesAsync();

        return written > 0 ? Result.Success() : Result.Error();
    }
    public async Task<Result> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var loadedUser = await GetUserByIdAsync(user.Id);

        if (!loadedUser.IsSuccess && loadedUser.Value is not null)
        {
            return Result.Error();
        }

        _dbContext.Update(user);

        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }
    public async Task<Result> DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        var loadedUser = await GetUserByIdAsync(user.Id);

        if (!loadedUser.IsSuccess && loadedUser.Value is not null)
        {
            return Result.Error();
        }

        _dbContext.Delete(user);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }
}
