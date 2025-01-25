using DailySpin.Application;
using DailySpin.Domain;
using DailySpin.WebApi;

public class WheelController : IController
{
    private readonly IWheelRepository _segments;

    public WheelController(IWheelRepository segments, IMissionRepository missions)
    {
        _segments = segments;
    }

    [HttpGet]
    [Authorize("User")]
    [ContentType(HttpContentType.Html)]
    [Route("/wheel")]
    public Task<byte[]> GetWheelPage()
    {
        return File.ReadAllBytesAsync("wwwroot/wheel.html");
    }

    [HttpGet]
    [Authorize("User")]
    [Route("api/wheel/segments")]
    public async Task<object> GetSegments([FromBody]UserDto user)
    {
        if (user is null)
            return new { message = "Invalid credentials" };

        var result = await _segments.GetAllSergmentsByIdAsync(user.Id);
    
        if (!result.IsSuccess)
            return new { message = "Not Found" };

        return result.Value;
    }

    [HttpPost]
    [Authorize("User")]
    [Route("api/wheel/spin")]
    public async Task<object> SpinWheel([FromBody]WheelSegment wheelResult)
    {
        await _segments.DeleteAsync(wheelResult);

        //await _missions.SetStatusAsync(Status.InProgress);
    
        return Task.CompletedTask;
    }
}

//public class WheelSegment
//{
//    public string Label { get; set; }
//    public string Color { get; set; }
//    public int Weight { get; set; }
//}

public record UserDto(Guid Id, string Name);