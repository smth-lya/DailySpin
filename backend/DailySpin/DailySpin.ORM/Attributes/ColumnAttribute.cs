namespace DailySpin.ORM;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class ColumnAttribute : Attribute
{
    public string Name { get; }

    public ColumnAttribute(string name)
    {
        Name = name;
    }
}
