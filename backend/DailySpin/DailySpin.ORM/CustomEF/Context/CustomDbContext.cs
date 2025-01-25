using System.Collections.Concurrent;
using System.Reflection;

namespace DailySpin.ORM;

public abstract class CustomDbContext : ICustomContext, IDisposable
{
    protected readonly ICustomDbConnection _connection;

    private readonly ConcurrentDictionary<object, EntityState> _changeTracker = new();

    private DatabaseFacade? _database;

    public CustomDbContext(ICustomDbConnection connection)
    {
        _connection = connection;

        InitializeSets();
    }

    public virtual DatabaseFacade Database
    {
        get
        {
            if (_database == null)
            {
                _database = new DatabaseFacade(_connection);
            }

            return _database;
        }
    }

    private void InitializeSets()
    {
        var setProperties = GetType().GetProperties()
            .Where(prop => prop.PropertyType.IsGenericType &&
                           prop.PropertyType.GetGenericTypeDefinition() == typeof(CustomDbSet<>));

        foreach (var prop in setProperties)
        {
            var entityType = prop.PropertyType.GetGenericArguments()[0];
            prop.SetValue(this, CreateSetInternal(entityType));
        }
    }

    private object CreateSetInternal(Type entityType)
    {
        var setType = typeof(CustomDbSet<>).MakeGenericType(entityType);
        return Activator.CreateInstance(setType, new CustomQueryProvider(this))!;
    }

    public string ResolveTableName(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        if (entityType.IsGenericType)
        {
            var innerType = entityType.GetGenericArguments().FirstOrDefault();
            if (innerType == null)
                throw new InvalidOperationException($"Generic тип {entityType.Name} не содержит аргументов.");

            return ResolveTableName(innerType);
        }

        var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();

        return tableAttribute?.Name ?? entityType.Name;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int written = 0;

        foreach (var (entity, state) in _changeTracker)
        {
            switch (state)
            {
                case EntityState.Added:
                    written += await InsertEntityAsync(entity, cancellationToken);
                    break;
                case EntityState.Modified:
                    written += await UpdateEntityAsync(entity, cancellationToken);
                    break;
                case EntityState.Deleted:
                    written += await DeleteEntityAsync(entity, cancellationToken);
                    break;
            }
        }

        _changeTracker.Clear();

        return written;
    }


    public void Add<T>(T entity) where T : class
        => TrackEntityState(entity, EntityState.Added);

    public void Update<T>(T entity) where T : class
        => TrackEntityState(entity, EntityState.Modified);

    public void Delete<T>(T entity) where T : class
        => TrackEntityState(entity, EntityState.Deleted);

    private void TrackEntityState(object entity, EntityState state)
    {
        _changeTracker[entity] = state;
    }


    private async Task<int> InsertEntityAsync(object entity, CancellationToken cancellationToken = default)
    {
        var sql = new NonQueryBuilder(this).Insert(entity);
        return await Database.ExecuteNonQueryAsync(sql, cancellationToken).ConfigureAwait(false);
    }

    private async Task<int> UpdateEntityAsync(object entity, CancellationToken cancellationToken = default)
    {
        var sql = new NonQueryBuilder(this).Update(entity);
        return await Database.ExecuteNonQueryAsync(sql, cancellationToken).ConfigureAwait(false);
    }

    private async Task<int> DeleteEntityAsync(object entity, CancellationToken cancellationToken = default)
    {
        var sql = new NonQueryBuilder(this).Insert(entity);
        return await Database.ExecuteNonQueryAsync(sql, cancellationToken);
    }

    public void Dispose()
    {
        
    }
}