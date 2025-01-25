using DailySpin.Domain;
using DailySpin.ORM;

namespace DailySpin.Infrastructure;

public class ApplicationDbContext : CustomDbContext
{
    public CustomDbSet<User> Users { get; private set; } = null!;
    public CustomDbSet<Mission> Missions { get; private set; } = null!;

    public CustomDbSet<WheelSegment> Segments { get; private set; } = null!;

    public ApplicationDbContext(ICustomDbConnection connection) : base(connection)
    { }
}
