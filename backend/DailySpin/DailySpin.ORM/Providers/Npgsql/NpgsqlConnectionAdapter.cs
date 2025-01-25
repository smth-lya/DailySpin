using Npgsql;
using System.Text.RegularExpressions;

namespace DailySpin.ORM;

public class NpgsqlConnectionAdapter : ICustomDbConnection, IDisposable, IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;

    public NpgsqlConnectionAdapter(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public ICustomCommand CreateCommand(FormattableString sql)
    {
        var command = _connection.CreateCommand();
        command.CommandText = ReplaceParamenters(sql.Format);

        for (int i = 0; i < sql.ArgumentCount; i++)
        {
            command.Parameters.AddWithValue($"@p{i}", sql.GetArgument(i));
        }

        return new NpgsqlCommandAdapter(command);
    }

    private static string ReplaceParamenters(string query)
    {
        var result = Regex.Replace(query, @"\{(\d+)\}", x => $"@p{x.Groups[1].Value}"); // {0} -> @p1
        return result;
    }

    public void Dispose()
        => _connection.Dispose();

    public async ValueTask DisposeAsync()
        => await _connection.DisposeAsync();
}
