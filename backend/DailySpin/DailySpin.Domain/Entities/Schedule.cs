namespace DailySpin.Domain;

public class Schedule
{
    public Guid Id { get; set; }
    public DateTime WorkDay { get; set; }
    public List<ScheduledTask> Tasks { get; set; }
}
public class ScheduledTask
{
    public Guid Id { get; set; }
    public Mission Mission { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
