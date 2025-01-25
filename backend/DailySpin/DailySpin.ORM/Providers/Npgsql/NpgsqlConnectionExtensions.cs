using Npgsql;

namespace DailySpin.ORM;

public static class NpgsqlConnectionExtensions
{
    public static ICustomDbConnection Adapt(this NpgsqlConnection connection)
    {
        return new NpgsqlConnectionAdapter(connection);
    }
}
