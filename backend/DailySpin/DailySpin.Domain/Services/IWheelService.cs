using Ardalis.Result;

namespace DailySpin.Domain;

public interface IWheelService
{
    Task<Result> SetSpinResultAsync(WheelSegment segment);
}
