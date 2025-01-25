using Ardalis.Result;

namespace DailySpin.Domain;

public interface IReadMissionRepository
{
    Task<Result<Mission>> GetByIdAsync(Guid id);
}
