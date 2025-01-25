using DailySpin.ORM;

internal class TestCustomContext : CustomDbContext
{
    public TestCustomContext(ICustomDbConnection connection)
        : base(connection)
    {
    }

    public CustomDbSet<TestEntity> TestSet { get; set; }
}
