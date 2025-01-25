using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DailySpin.ORM;

public class QueryBuilder : ExpressionVisitor
{
    private Expression? _selectList, _whereExpression;
    private readonly ICustomContext _context;
    private bool _isFirstOrDefault;

    public QueryBuilder(ICustomContext context)
    {
        _context = context;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.IsGenericMethod)
        {
            var genericMethod = node.Method.GetGenericMethodDefinition();

            if (genericMethod == QueryableMethods.Select)
            {
                ExtractExpression(node, out _selectList);
            }
            else if (genericMethod == QueryableMethods.Where)
            {
                ExtractExpression(node, out _whereExpression);
            }
            else if (genericMethod == QueryableMethods.FirstOrDefaultWithPredicate)
            {
                ExtractExpression(node, out var predicate);

                _whereExpression = _whereExpression is null
                    ? predicate
                    : Expression.AndAlso(_whereExpression, predicate);

                _isFirstOrDefault = true;
            }
            else if (genericMethod == QueryableMethods.FirstOrDefaultWithoutPredicate)
            {
                _isFirstOrDefault = true;
            }
        }

        return base.VisitMethodCall(node);
    }

    private void ExtractExpression(MethodCallExpression node, out Expression target)
    {
        target = ((UnaryExpression)node.Arguments[1]).Operand;
    }

    public FormattableString Compile(Expression expression)
    {
        Visit(expression);

        var whereVisitor = new WhereVisitor();
        whereVisitor.Visit(_whereExpression);

        var selectVisitor = new SelectVisitor();
        selectVisitor.Visit(_selectList);

        var whereClause = whereVisitor.Result;
        var selectClause = selectVisitor.Result;

        var tableName = _context.ResolveTableName(expression.Type);

        var sqlBuilder = new StringBuilder()
          .AppendLine("SELECT")
          .AppendLine($"    {selectClause ?? "*"}")
          .AppendLine("FROM")
          .AppendLine($"    {tableName}");

        if (!string.IsNullOrWhiteSpace(whereClause))
        {
            sqlBuilder.AppendLine("WHERE")
                      .AppendLine($"    {whereClause}");
        }

        if (_isFirstOrDefault)
            sqlBuilder.AppendLine("LIMIT 1");

        return FormattableStringFactory.Create(sqlBuilder.ToString());
    }
}


internal class StringExpression : Expression
{
    public string String { get; set; }

    public StringExpression(string @string, ExpressionType nodeType, Type type)
    {
        String = @string;
        NodeType = nodeType;
        Type = type;
    }

    public override ExpressionType NodeType { get; }
    public override Type Type { get; }
}

internal class WhereVisitor : ExpressionVisitor
{
    protected override Expression VisitBinary(BinaryExpression node)
    {
        var @operator = node.NodeType switch
        {
            ExpressionType.GreaterThan => ">",
            ExpressionType.Equal => "=",
            ExpressionType.OrElse => "OR",
            ExpressionType.AndAlso => "AND",
            ExpressionType.LessThan => "<",
            _ => throw new NotSupportedException()
        };

        var left = ToString(ExtractExpression(node.Left));
        var right = ToString(ExtractExpression(node.Right));


        Result = $"{left} {@operator} {right}";

        return base.VisitBinary(node);
    }

    public string? Result { get; set; }

    private string? ToString(Expression expression)
    {
        if (expression is ConstantExpression ce)
            return $"'{ce.Value?.ToString()}'";

        if (expression is MemberExpression me)
        {
            var columnAttribute = me.Member.GetCustomAttribute<ColumnAttribute>();

            return columnAttribute?.Name ?? me.Member.Name;
        }
        return expression.ToString();
    }

    private Expression ExtractExpression(Expression expression)
    {
        while (expression is MemberExpression memberExpression &&
               memberExpression.Expression is ConstantExpression constantExpression)
        {
            var value = constantExpression.Value;

            var memberName = memberExpression.Member.Name;
            var fieldInfo = value?.GetType().GetField(memberName);

            if (fieldInfo != null)
            {
                var extractedValue = fieldInfo.GetValue(value);

                if (extractedValue is Expression extractedExpression)
                {
                    expression = extractedExpression;
                    continue;
                }
                else
                {
                    return Expression.Constant(extractedValue);
                }
            }

            break;
        }

        return expression;
    }
}

internal class SelectVisitor : ExpressionVisitor
{
    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        var nodes = node.Bindings.Cast<MemberAssignment>().Select(x => ToString(x.Expression));
        Result = string.Join(", ", nodes);
        return base.VisitMemberInit(node);
    }

    public string? Result { get; set; }

    private string? ToString(Expression expression)
    {
        if (expression is ConstantExpression ce)
            return $"'{ce.Value?.ToString()}'";

        if (expression is MemberExpression me)
        {
            var columnAttribute = me.Member.GetCustomAttribute<ColumnAttribute>();

            return columnAttribute?.Name ?? me.Member.Name;
        }

        return expression.ToString();
    }
}