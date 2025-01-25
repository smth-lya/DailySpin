using DailySpin.ORM;
using System.Diagnostics.CodeAnalysis;

namespace DailySpin.Domain;

[Table("missions")]
public sealed class Mission
{
    public Guid Id { get; set; }

    public required string Name { get; init; }
    public string? Description { get; init; }

    public required int Priority { get; init; }
    public required int Difficulty { get; init; }
    public required int Reward { get; init; }
    
    public required Guid OwnderId { get; init; }
    public required Status Status { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime CompletedAt { get; set; }
    public TimeSpan TimeLimit { get; init; }

    public bool IsCompleted { get; set; }

    public Mission() { }

    [SetsRequiredMembers]
    public Mission(string name, int priority, int difficulty, int reward)
    {
        Name = name;
        Priority = priority;
        Difficulty = difficulty;
        Reward = reward;

        CreatedAt = DateTime.UtcNow;
    }
}

public class WheelSegment
{
    public Guid Id { get; init; }
    public Guid MissionId { get; init; }
    public double Weight { get; init; }
}

public enum Priority
{
    Low,
    Medium,
    High
}
public enum Status
{
    New,
    InProgress,
    Completed,
    Cancelled,
    Deleted
}
