using Ardalis.Result;
using DailySpin.Domain;

namespace DailySpin.Infrastructure;

public class WheelRepository : IWheelRepository
{
    private ApplicationDbContext _dbContext;

    public WheelRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Result<WheelSegment>> AddAsync(WheelSegment entity)
    {
        throw new NotImplementedException();
    }

    public Task<Result<WheelSegment>> DeleteAsync(WheelSegment entity)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<List<WheelSegment>>> GetAllSergmentsByIdAsync(Guid userId)
    {
        return Result.Success(_dbContext.Segments.ToList());
    }

    public Task<Result<WheelSegment>> UpdateAsync(WheelSegment entity)
    {
        throw new NotImplementedException();
    }

    public Task<Result<WheelSegment>> UpdateAsync(Func<WheelSegment, bool> predicate, Action<WheelSegment> updateAction)
    {
        throw new NotImplementedException();
    }
}
