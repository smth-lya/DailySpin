//using DailySpin.Domain;

//public class GenerateScheduleUseCase
//{
//    private readonly ITaskRepository _taskRepository;
//    private readonly IScheduleRepository _scheduleRepository;

//    public GenerateScheduleUseCase(ITaskRepository taskRepository, IScheduleRepository scheduleRepository)
//    {
//        _taskRepository = taskRepository;
//        _scheduleRepository = scheduleRepository;
//    }

//    public Schedule Execute(DateTime workDay, TimeSpan workStart, TimeSpan workEnd)
//    {
//        var tasks = _taskRepository.GetPendingTasks();
//        var schedule = new Schedule
//        {
//            Id = Guid.NewGuid(),
//            WorkDay = workDay,
//            Tasks = new List<ScheduledTask>()
//        };

//        var availableTime = workEnd - workStart;

//        foreach (var task in tasks.OrderByDescending(t => t.Priority))
//        {
//            if (task.EstimatedTime <= availableTime)
//            {
//                schedule.Tasks.Add(new ScheduledTask
//                {
//                    Id = Guid.NewGuid(),
//                    Task = task,
//                    StartTime = workStart,
//                    EndTime = workStart + task.EstimatedTime
//                });
//                workStart += task.EstimatedTime;
//                availableTime -= task.EstimatedTime;
//            }
//        }

//        _scheduleRepository.Save(schedule);
//        return schedule;
//    }
//}
