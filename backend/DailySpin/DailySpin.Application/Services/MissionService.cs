//using DailySpin.Domain;

//namespace DailySpin.Application;

//public class MissionService : IMissionService
//{
//    private readonly IMissionRepository _missionRepository;

//    public MissionService(IMissionRepository missionRepository)
//    {
//        _missionRepository = missionRepository;
//    }

//    public async Task<bool> CreateMissionAsync(MissionDto dto)
//    {
//        var mission = MissionMapper.MapToDomain(dto);
//        return await _missionRepository.AddAsync(mission);
//    }

//    public async Task<bool> UpdateMissionAsync(Guid id, MissionDto dto)
//    {
//        var mission = await _missionRepository.GetByIdAsync(id);
//        if (mission == null) return false;

//        var updatedMission = MissionMapper.MapToDomain(dto, mission);
//        return await _missionRepository.UpdateAsync(updatedMission);
//    }

//    public async Task<bool> DeleteMissionAsync(Guid id)
//    {
//        var mission = await _missionRepository.GetByIdAsync(id);
//        if (mission == null) return false;

//        mission.ChangeStatus(MissionStatus.Deleted);
//        return await _missionRepository.UpdateAsync(mission);
//    }

//    public async Task<IEnumerable<MissionDto>> GetMissionsAsync(Guid userId)
//    {
//        var missions = await _missionRepository.GetAllAsync(m => m.UserId == userId);
//        return missions.Select(MissionMapper.MapToDto);
//    }
//}
//public record MissionDto(string Name, string Description, Priority Priority, int Difficulty, int Reward, TimeSpan TimeLimit, bool IsPausedAllowed);
//public static class MissionMapper
//{
//    public static Mission MapToDomain(MissionDto dto, Mission? existingMission = null)
//    {
//        return existingMission is null
//            ? new Mission(
//                Guid.NewGuid(),
//                dto.UserId,
//                dto.Name,
//                dto.Description,
//                dto.Priority,
//                dto.Difficulty,
//                dto.Reward,
//                dto.TimeLimit,
//                dto.IsPausedAllowed,
//                MissionStatus.Active)
//            : existingMission with
//            {
//                Name = dto.Name,
//                Description = dto.Description,
//                Priority = dto.Priority,
//                Difficulty = dto.Difficulty,
//                Reward = dto.Reward,
//                TimeLimit = dto.TimeLimit,
//                IsPausedAllowed = dto.IsPausedAllowed
//            };
//    }

//    public static MissionDto MapToDto(Mission mission) =>
//        new(
//            mission.UserId,
//            mission.Name,
//            mission.Description,
//            mission.Priority,
//            mission.Difficulty,
//            mission.Reward,
//            mission.TimeLimit,
//            mission.IsPausedAllowed);
//}