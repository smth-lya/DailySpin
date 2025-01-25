//using DailySpin.Domain;

//namespace DailySpin.Application
//{
//    public class WheelService : IWheelService
//    {
//        private readonly IWheelRepository _repository;

//        public WheelService(IWheelRepository repository)
//        {
//            _repository = repository;
//        }

//        public async Task<Guid> SpinWheelAsync(Guid userId)
//        {
//            var result = await _repository.GetAllSergmentsById(userId);

//            if (!result.IsSuccess)
//                throw new InvalidOperationException("No missions available to spin.");

//            var segments = result.Value;

//            var totalWeight = segments.Sum(s => s.Weight);
//            var target = Random.Shared.NextDouble() * totalWeight;

//            foreach (var segment in segments)
//            {
//                target -= segment.Weight;
//                if (target <= 0)
//                    return segment.MissionId;
//            }

//            throw new InvalidOperationException("No missions available to spin.");
//        }
//    }
//}
