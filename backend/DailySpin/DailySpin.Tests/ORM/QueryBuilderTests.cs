using DailySpin.ORM;
using Moq;
using System.Linq.Expressions;


public class QueryBuilderTests
{
    private readonly Mock<ICustomContext> _mockContext;
    private readonly QueryBuilder _builder;

    public QueryBuilderTests()
    {
        _mockContext = new Mock<ICustomContext>();
        _builder = new QueryBuilder(_mockContext.Object);
    }

    [Fact]
    public void Compile_SelectExpression_ShouldGenerateCorrectSql()
    {
        _mockContext.Setup(x => x.ResolveTableName(It.IsAny<Type>())).Returns("TestEntities");

        Expression<Func<TestEntity, object>> expression = x => new TestEntity { Id = x.Id, Name = x.Name };

        var query = Queryable.Select(new TestEntity[] { }.AsQueryable(), expression);

        var result = _builder.Compile(query.Expression);

        var expectedQuery = """
            SELECT
                "Id", "Name"
            FROM
                TestEntities
            """;

        var actualQuery = result.Format.NormalizeSql();
        expectedQuery = expectedQuery.NormalizeSql();

        Assert.Equal(actualQuery, expectedQuery);
        _mockContext.Verify(x => x.ResolveTableName(It.IsAny<Type>()), Times.Once);
    }

    [Fact]
    public void Compile_WhereExpression_ShouldGenerateCorrectSql()
    {
        _mockContext.Setup(x => x.ResolveTableName(It.IsAny<Type>())).Returns("TestEntities");

        Expression<Func<TestEntity, bool>> expression = x => x.Id < 5;
        var query = Queryable.Where(new TestEntity[] { }.AsQueryable(), expression);

        var result = _builder.Compile(query.Expression);

        var expectedQuery = """
            SELECT
                *
            FROM
                TestEntities
            WHERE
                "Id" < 5
            """;

        var actualQuery = result.Format.NormalizeSql();
        expectedQuery = expectedQuery.NormalizeSql();

        Assert.Equal(actualQuery, expectedQuery);
        _mockContext.Verify(x => x.ResolveTableName(It.IsAny<Type>()), Times.Once);
    }

    [Fact]
    public void Compile_SelectAndWhereExpression_ShouldGenerateCorrectSql()
    {
        _mockContext.Setup(x => x.ResolveTableName(It.IsAny<Type>())).Returns("TestEntities");

        Expression<Func<TestEntity, TestEntity>> selectExpression = x => new TestEntity { Id = x.Id, Name = x.Name };
        Expression<Func<TestEntity, bool>> whereExpression = x => x.Id < 5;

        var query = new TestEntity[] { }.AsQueryable().Select(selectExpression).Where(whereExpression);

        var result = _builder.Compile(query.Expression);

        var expectedQuery = """
            SELECT
                "Id", "Name"
            FROM
                TestEntities
            WHERE
                "Id" < 5
            """;

        var actualQuery = result.Format.NormalizeSql();
        expectedQuery = expectedQuery.NormalizeSql();

        Assert.Equal(actualQuery, expectedQuery);
        _mockContext.Verify(x => x.ResolveTableName(It.IsAny<Type>()), Times.Once);
    }
}
