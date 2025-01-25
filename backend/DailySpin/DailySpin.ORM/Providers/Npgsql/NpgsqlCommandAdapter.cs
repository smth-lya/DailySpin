using Npgsql;
using System.Data;

namespace DailySpin.ORM;

public class NpgsqlCommandAdapter : ICustomCommand
{
    private readonly NpgsqlCommand _command;

    public NpgsqlCommandAdapter(NpgsqlCommand command)
    {
        _command = command;
    }
    public async Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default)
    {
        return await _command.ExecuteReaderAsync(cancellationToken);
    }

    public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
    {
        return await _command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await _command.DisposeAsync();
    }
}