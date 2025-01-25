namespace DailySpin.ORM;

public interface ICustomContext
{
    abstract DatabaseFacade Database { get; }

    string ResolveTableName(Type entityType);
    void Add<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Delete<T>(T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
