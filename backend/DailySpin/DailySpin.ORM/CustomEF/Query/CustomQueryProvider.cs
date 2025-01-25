using System.Linq.Expressions;

namespace DailySpin.ORM;
public interface IAsyncQueryProvider : IQueryProvider
{
    Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken);
    Task<object?> ExecuteAsync(Expression expression, CancellationToken cancellationToken);
}

public class CustomQueryProvider : IAsyncQueryProvider
{
    private readonly ICustomContext _context;

    public CustomQueryProvider(ICustomContext context)
    {
        _context = context;
    }


    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new CustomQueryable<TElement>(expression, this);
    }

    public IQueryable CreateQuery(Expression expression)
        => CreateQuery<object>(expression);


    public TResult Execute<TResult>(Expression expression)
    {
        var result = new QueryBuilder(_context);
        var sql = result.Compile(expression);

        return _context.Database.ExecuteQueryAsync<TResult>(sql).GetAwaiter().GetResult();
    }

    public object? Execute(Expression expression)
        => Execute<object>(expression);


    public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var result = new QueryBuilder(_context);
        var sql = result.Compile(expression);

        return await _context.Database.ExecuteQueryAsync<TResult>(sql, cancellationToken);
    }

    public async Task<object?> ExecuteAsync(Expression expression, CancellationToken cancellationToken = default)
        => await ExecuteAsync<object>(expression, cancellationToken);
}
