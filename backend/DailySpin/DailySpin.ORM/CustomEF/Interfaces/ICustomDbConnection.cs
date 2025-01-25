namespace DailySpin.ORM;

public interface ICustomDbConnection
{
    ICustomCommand CreateCommand(FormattableString sql);
}
