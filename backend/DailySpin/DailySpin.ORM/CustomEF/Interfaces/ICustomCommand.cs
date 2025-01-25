using System.Data;

namespace DailySpin.ORM;

public interface ICustomCommand : IAsyncDisposable
{
    Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default);
    Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default);
}
