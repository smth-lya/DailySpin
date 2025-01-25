using Ardalis.Result;
using DailySpin.Domain;
using DailySpin.ORM;

namespace DailySpin.Infrastructure;

public class MissionRepository : IMissionRepository
{
    private ApplicationDbContext _dbContext;

    public MissionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Mission>> AddAsync(Mission mission)
    {
        var loadedMission = _dbContext.Missions.FirstOrDefault(m => m.Id == m.Id);

        if (loadedMission is not null)
        {
            return Result.Error();
        }

        _dbContext.Add(mission);

        var written = await _dbContext.SaveChangesAsync();

        return written > 0 ? Result.Success(mission) : Result.Error();
    }

    public async Task<Result<Mission>> UpdateAsync(Mission mission)
    {
        var loadedMission = _dbContext.Missions.FirstOrDefault(m => m.Id == m.Id);

        if (loadedMission is null)
        {
            return Result.NotFound();
        }

        _dbContext.Add(mission);

        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<Mission>> DeleteAsync(Mission mission)
    {
        var loadedMission = _dbContext.Missions.FirstOrDefault(m => m.Id == m.Id);

        if (loadedMission is null)
        {
            return Result.NotFound();
        }

        _dbContext.Delete(mission);
        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<Mission>> GetByIdAsync(Guid id)
    {
        var mission = await _dbContext.Missions.FirstOrDefaultAsync(m => m.Id == id); 
        return mission is null ? Result<Mission>.NotFound() : Result.Success(mission);
    }
}