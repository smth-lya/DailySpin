using System.Collections;
using System.Linq.Expressions;

namespace DailySpin.ORM;

public class CustomQueryable<T> : IQueryable<T>, IAsyncEnumerable<T>
{
    public CustomQueryable(IAsyncQueryProvider provider)
    {
        Expression = Expression.Constant(this);
        Provider = provider;
        ElementType = typeof(T);
    }

    public CustomQueryable(Expression expression, CustomQueryProvider customQueryProvider)
    {
        Expression = expression;
        Provider = customQueryProvider;
        ElementType = typeof(T);
    }

    public Type ElementType { get; }

    public Expression Expression { get; }

    public IQueryProvider Provider { get; }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var results = await ((IAsyncQueryProvider)Provider).ExecuteAsync<IEnumerable<T>>(Expression, cancellationToken).ConfigureAwait(false);
        foreach (var item in results)
        {
            yield return item;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}