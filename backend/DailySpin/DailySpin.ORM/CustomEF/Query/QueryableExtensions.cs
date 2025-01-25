using System.Linq.Expressions;
using System.Reflection;

namespace DailySpin.ORM;

public static class QueryableExtensions
{
    public static async Task<TSource?> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync<TSource, TSource>(QueryableMethods.FirstOrDefaultWithPredicate, source, predicate, cancellationToken);
    }
    private static async Task<TResult> ExecuteAsync<TSource, TResult>(MethodInfo operatorMethodInfo, IQueryable<TSource> source, Expression expression, CancellationToken cancellationToken = default)
    {
        if (source.Provider is IAsyncQueryProvider asyncQueryProvider)
        {
            if (operatorMethodInfo.IsGenericMethod)
            {
                operatorMethodInfo = operatorMethodInfo.GetGenericArguments().Length == 2 ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single()) : operatorMethodInfo.MakeGenericMethod(typeof(TSource));
            }

            return await asyncQueryProvider.ExecuteAsync<TResult>(
                Expression.Call(null, operatorMethodInfo, expression != null
                ? [source.Expression, expression]
                : [source.Expression]), cancellationToken);
        }

        throw new InvalidOperationException();
    }

    private static Task<TResult> ExecuteAsync<TSource, TResult>(MethodInfo operatorMethodInfo, IQueryable<TSource> source, LambdaExpression expression, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync<TSource, TResult>(operatorMethodInfo, source, Expression.Quote(expression), cancellationToken);
    }

    private static Task<TResult> ExecuteAsync<TSource, TResult>(MethodInfo operatorMethodInfo, IQueryable<TSource> source, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync<TSource, TResult>(operatorMethodInfo, source, (Expression)null, cancellationToken);
    }
}
