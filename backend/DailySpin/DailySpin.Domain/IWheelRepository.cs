using Ardalis.Result;

namespace DailySpin.Domain;

public interface IWheelRepository : IReadWheelRepository
{
    Task<Result<WheelSegment>> AddAsync(WheelSegment entity);
    Task<Result<WheelSegment>> UpdateAsync(WheelSegment entity);
    Task<Result<WheelSegment>> UpdateAsync(Func<WheelSegment, bool> predicate, Action<WheelSegment> updateAction);
    Task<Result<WheelSegment>> DeleteAsync(WheelSegment entity);
}

public interface IReadWheelRepository
{
    Task<Result<List<WheelSegment>>> GetAllSergmentsByIdAsync(Guid userId);
}
