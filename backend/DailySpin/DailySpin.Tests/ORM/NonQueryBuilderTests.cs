using DailySpin.ORM;
using Moq;
using System.Linq.Expressions;

public class NonQueryBuilderTests
{
    private Mock<ICustomContext> _mockContext;
    private NonQueryBuilder _builder;

    public NonQueryBuilderTests()
    {
        _mockContext = new Mock<ICustomContext>();
        _builder = new NonQueryBuilder(_mockContext.Object);
    }

    [Fact]
    public void Insert_ValidEntity_ReturnCorrectSql()
    {
        _mockContext.Setup(x => x.ResolveTableName(It.IsAny<Type>())).Returns("TestEntities");

        var testEntity = new TestEntity { Id = 3, Name = "John" };

        var result = _builder.Insert(testEntity);

        var expectedQuery = """
            INSERT INTO TestEntities ("Id", "Name")
            VALUES ('3', 'John');
            """;

        var actualQuery = result.Format.NormalizeSql();
        expectedQuery = expectedQuery.NormalizeSql();

        Assert.Equal(actualQuery, expectedQuery);
        _mockContext.Verify(x => x.ResolveTableName(It.IsAny<Type>()), Times.Once);
    }

    [Fact]
    public void Update_ValidEntity_ReturnCorrectSql()
    {
        _mockContext.Setup(x => x.ResolveTableName(It.IsAny<Type>())).Returns("TestEntities");

        var testEntity = new TestEntity { Id = 1, Name = "Sam" };
        var result = _builder.Update(testEntity);

        var expectedQuery = """
            UPDATE 
                TestEntities
            SET
                "Id" = '1',
                "Name" = 'Sam';
            """;

        var actualQuery = result.Format.NormalizeSql();
        expectedQuery = expectedQuery.NormalizeSql();

        Assert.Equal(actualQuery, expectedQuery);
        _mockContext.Verify(x => x.ResolveTableName(It.IsAny<Type>()), Times.Once);
    }

    [Fact]
    public void UpdateWhere_ValidEntity_ReturnCorrectSql()
    {
        _mockContext.Setup(x => x.ResolveTableName(It.IsAny<Type>())).Returns("TestEntities");

        Expression<Func<TestEntity, bool>> predicate = x => x.Id < 5;
        Action<TestEntity> updateAction = x => x.Name = "Bob";

        var result = _builder.Update(predicate, updateAction);

        var expectedQuery = """
            UPDATE 
                TestEntities
            SET
                "Id" = '0',
                "Name" = 'Bob'
            WHERE 
                "Id" < 5;
            """;

        var actualQuery = result.Format.NormalizeSql();
        expectedQuery = expectedQuery.NormalizeSql();

        Assert.Equal(actualQuery, expectedQuery);
        _mockContext.Verify(x => x.ResolveTableName(It.IsAny<Type>()), Times.Once);
    }

    [Fact]
    public void Delete_ValidEntity_ReturnCorrectSql()
    {
        _mockContext.Setup(x => x.ResolveTableName(It.IsAny<Type>())).Returns("TestEntities");

        Expression<Func<TestEntity, bool>> predicate = x => x.Id < 5;

        // Act
        var result = _builder.Delete(predicate);

        var expectedQuery = """
            DELETE FROM
                TestEntities
            WHERE
                "Id" < 5;
            """;

        var actualQuery = result.Format.NormalizeSql();
        expectedQuery = expectedQuery.NormalizeSql();

        Assert.Equal(actualQuery, expectedQuery);
        _mockContext.Verify(x => x.ResolveTableName(It.IsAny<Type>()), Times.Once);
    }
}