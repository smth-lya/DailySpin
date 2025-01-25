using DailySpin.ORM;

public class TestEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

[Table("CustomTableName")]
public class TestEntityWithTableAttribute
{
    public int Id { get; set; }
    public string? Name { get; set; }
}