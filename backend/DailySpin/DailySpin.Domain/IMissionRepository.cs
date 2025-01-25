using Ardalis.Result;

namespace DailySpin.Domain;

public interface IMissionRepository
{
    Task<Result<Mission>> AddAsync(Mission mission);
    Task<Result<Mission>> UpdateAsync(Mission mission);
    Task<Result<Mission>> DeleteAsync(Mission mission);
}
