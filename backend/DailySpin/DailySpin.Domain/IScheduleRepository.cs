using Ardalis.Result;

namespace DailySpin.Domain;

public interface IScheduleRepository
{
    Task<Result<Schedule>> GetByDateAsync(DateTime date);

    Task<Result<Schedule>> GetByIdAsync(int id);

    Task<Result> SaveAsync(Schedule schedule);

    Task<Result> DeleteAsync(int id);
}