using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DailySpin.ORM;

public class NonQueryBuilder
{
    private readonly ICustomContext _context;

    public NonQueryBuilder(ICustomContext context)
    {
        _context = context;
    }

    public FormattableString Insert<T>(T entity)
    {
        var type = entity.GetType();
        var tableName = _context.ResolveTableName(type);
        var properties = type.GetProperties();
        var columns = string.Join(", ", properties.Select(p => $"{p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name}"));
        var values = string.Join(", ", properties.Select(p => $"'{p.GetValue(entity)}'"));

        var sqlBuilder = new StringBuilder()
            .AppendLine($"INSERT INTO {tableName} ({columns})")
            .AppendLine($"VALUES ({values});");

        return FormattableStringFactory.Create(sqlBuilder.ToString());
    }

    public FormattableString Update<T>(T entity)
    {
        var type = entity.GetType();
        var tableName = _context.ResolveTableName(type);
        var properties = type.GetProperties();
        var setClause = string.Join(", ", properties.Select(p => $"{p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name} = '{p.GetValue(entity)}'"));

        var sqlBuilder = new StringBuilder()
            .AppendLine($"UPDATE {tableName}")
            .AppendLine($"SET {setClause};");

        return FormattableStringFactory.Create(sqlBuilder.ToString());
    }

    public FormattableString Update<T>(Expression<Func<T, bool>> predicate, Action<T> updateAction)
    {
        var tableName = _context.ResolveTableName(typeof(T));
        var entity = Activator.CreateInstance<T>();
        updateAction(entity);

        var properties = typeof(T).GetProperties();
        var setClause = string.Join(", ", properties.Select(p => $"{p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name} = '{p.GetValue(entity)}'"));

        var whereVisitor = new WhereVisitor();
        whereVisitor.Visit(predicate);

        var whereClause = whereVisitor.Result;

        var sqlBuilder = new StringBuilder()
            .AppendLine($"UPDATE {tableName}")
            .AppendLine($"SET {setClause}")
            .AppendLine($"WHERE {whereClause};");

        return FormattableStringFactory.Create(sqlBuilder.ToString());
    }

    public FormattableString Delete<T>(Expression<Func<T, bool>> predicate)
    {
        var tableName = _context.ResolveTableName(typeof(T));

        var whereVisitor = new WhereVisitor();
        whereVisitor.Visit(predicate);

        var whereClause = whereVisitor.Result;

        var sqlBuilder = new StringBuilder()
            .AppendLine($"DELETE FROM {tableName}")
            .AppendLine($"WHERE {whereClause};");

        return FormattableStringFactory.Create(sqlBuilder.ToString());
    }
}

