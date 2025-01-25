using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace DailySpin.ORM;

public class DatabaseFacade
{
    private static readonly MethodInfo
       GetStringMethod = typeof(DataReaderExtensions).GetMethod("GetString")!,
       GetInt32Method = typeof(DataReaderExtensions).GetMethod("GetInt32")!,
       GetGuidMethod = typeof(DataReaderExtensions).GetMethod("GetGuid")!,
       GetDateTimeMethod = typeof(DataReaderExtensions).GetMethod("GetDateTime")!;

    private static ConcurrentDictionary<Type, Delegate> _mapperFuncs = new();

    private readonly ICustomDbConnection _connection;

    public DatabaseFacade(ICustomDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<int> ExecuteNonQueryAsync(
        FormattableString sql,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(_connection, nameof(_connection));
        ArgumentNullException.ThrowIfNull(sql, nameof(sql));

        await using var command = _connection.CreateCommand(sql);
        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> ExecuteQueryAsync<T>(
        FormattableString sql,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteQueryAsync(sql, typeof(T), cancellationToken).ConfigureAwait(false);

        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return (T)result;

        return result.Count > 0 ? (T)result[0] : default;
    }

    public async Task<IList> ExecuteQueryAsync(
        FormattableString sql,
        Type entityType,
        CancellationToken cancellationToken = default)
    {
        var entityInnerType = entityType;

        bool isIEnumerable = entityType.IsGenericType && entityType.GetGenericTypeDefinition() == typeof(IEnumerable<>);

        if (isIEnumerable)
            entityInnerType = entityType.GetGenericArguments()[0];

        var result = await InternalExecuteQueryAsync(sql, entityInnerType, cancellationToken).ConfigureAwait(false);

        if (isIEnumerable)
            return result;

        return result.Count > 0 ? [result[0]] : new List<object>();
    }

    private async Task<List<T>> InternalExecuteQueryAsync<T>(
        FormattableString sql,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_connection, nameof(_connection));
        ArgumentNullException.ThrowIfNull(sql, nameof(sql));

        await using var command = _connection.CreateCommand(sql);
        using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<T>();
        var mapper = (Func<IDataReader, T>)_mapperFuncs.GetOrAdd(typeof(T), _ => Build<T>());

        if (reader is DbDataReader dbReader)
        {
            while (await dbReader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                results.Add(mapper(dbReader));
            }
        }
        else
        {
            while (reader.Read())
            {
                results.Add(mapper(reader));
            }
        }

        return results;
    }

    private async Task<IList> InternalExecuteQueryAsync(
        FormattableString sql,
        Type entityType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_connection, nameof(_connection));
        ArgumentNullException.ThrowIfNull(sql, nameof(sql));
        ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));

        var genericQueryMethod = typeof(DatabaseFacade).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == nameof(InternalExecuteQueryAsync) && m.IsGenericMethod)?
            .MakeGenericMethod(entityType);

        if (genericQueryMethod == null)
            throw new InvalidOperationException($"Метод ExecuteQueryAsync для типа {entityType.Name} не найден.");

        var task = genericQueryMethod.Invoke(this, [sql, cancellationToken]) as Task;

        if (task == null)
            throw new InvalidOperationException($"Метод ExecuteQueryAsync не вернул корректный Task для типа {entityType.Name}.");

        await task.ConfigureAwait(false);

        var resultProperty = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);

        if (resultProperty == null)
            throw new InvalidOperationException("Свойство Result отсутствует у задачи.");

        var result = resultProperty.GetValue(task);

        return result as IList ?? new List<object?> { result };
    }

    private Func<IDataReader, T> Build<T>()
    {
        var readerParam = Expression.Parameter(typeof(IDataReader));

        var newExp = Expression.New(typeof(T));
        var memberInit = Expression.MemberInit(newExp, typeof(T).GetProperties()
            .Select(x => Expression.Bind(x, BuildReadColumnExpression(readerParam, x))));

        return Expression.Lambda<Func<IDataReader, T>>(memberInit, readerParam).Compile();
    }

    private Expression BuildReadColumnExpression(Expression reader, PropertyInfo prop)
    {
        var columnName = prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name;

        if (prop.PropertyType == typeof(string))
            return Expression.Call(null, GetStringMethod, reader, Expression.Constant(columnName));
        else if (prop.PropertyType == typeof(int))
            return Expression.Call(null, GetInt32Method, reader, Expression.Constant(columnName));
        else if (prop.PropertyType == typeof(Guid))
            return Expression.Call(null, GetGuidMethod, reader, Expression.Constant(columnName));
        else if (prop.PropertyType == typeof(DateTime))
            return Expression.Call(null, GetDateTimeMethod, reader, Expression.Constant(columnName));

        throw new NotSupportedException($"Тип '{prop.PropertyType}' не поддерживается для чтения.");
    }
}