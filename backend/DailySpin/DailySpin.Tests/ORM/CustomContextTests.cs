using DailySpin.ORM;
using Moq;

public class CustomContextTests
{
    private readonly Mock<ICustomDbConnection> _mockConnection;
    private readonly Mock<DatabaseFacade> _mockDatabase;
    private readonly CustomDbContext _context;

    public CustomContextTests()
    {
        _mockConnection = new Mock<ICustomDbConnection>();
        _mockDatabase = new Mock<DatabaseFacade>(_mockConnection.Object);
        _context = new TestCustomContext(_mockConnection.Object);
    }

    [Fact]
    public void CustomContext_InitializeSets_ShouldInitializeSetsCorrectly()
    {
        // Arrange
        var mockConnection = new Mock<ICustomDbConnection>();
        var context = new TestCustomContext(mockConnection.Object); 

        // Act
        var setProperties = context.GetType().GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(CustomDbSet<>));

        // Assert
        Assert.True(setProperties.Any(), "Sets should be initialized.");
        foreach (var property in setProperties)
        {
            var value = property.GetValue(context);
            Assert.NotNull(value);
            Assert.IsAssignableFrom(typeof(CustomDbSet<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0]), value);
        }
    }

    [Fact]
    public void ResolveTableName_ShouldReturnCorrectTableName()
    {
        // Act
        var tableName = _context.ResolveTableName(typeof(TestEntity));

        // Assert
        Assert.Equal("TestEntity", tableName);
    }

    [Fact]
    public void ResolveTableName_ShouldReturnCorrectTableName_FromInnerGenericType()
    {
        // Act
        var tableName = _context.ResolveTableName(typeof(IEnumerable<TestEntity>));

        // Assert
        Assert.Equal("TestEntity", tableName);
    }
    
    [Fact]
    public void ResolveTableName_ShouldReturnCustomTableNameFromAttribute()
    {
        // Act
        var tableName = _context.ResolveTableName(typeof(TestEntityWithTableAttribute));

        // Assert
        Assert.Equal("CustomTableName", tableName);
    }

    [Fact]
    public void ResolveTableName_ShouldReturnCustomTableNameFromAttribute_FromInnerGenericType()
    {
        // Act
        var tableName = _context.ResolveTableName(typeof(IEnumerable<TestEntityWithTableAttribute>));

        // Assert
        Assert.Equal("CustomTableName", tableName);
    }

    [Fact]
    public void ResolveTableName_ShouldThrowExceptionWithInvalidType()
    {
        //Act & Assert
        Assert.Throws<ArgumentNullException>(() => _context.ResolveTableName(null!));
    }

    //[Fact]
    //public async Task CustomContext_SaveChangesAsync_HandlesLargeNumberOfEntities()
    //{
    //    // Arrange
    //    var mockConnection = new Mock<ICustomConnection>(); 
    //    var mockDatabaseFacade = new Mock<DatabaseFacade>(mockConnection.Object);
    //    var context = new TestCustomContext(mockConnection.Object);
    //    var entities = Enumerable.Range(0, 1000).Select(i => new TestEntity()).ToList();

    //    foreach (var entity in entities)
    //    {
    //        context.Add(entity);
    //    }

    //    //mockDatabaseFacade.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<FormattableString>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

    //    // Act
    //    var stopwatch = Stopwatch.StartNew();
    //    await context.SaveChangesAsync();
    //    stopwatch.Stop();

    //    // Assert
    //    Assert.True(stopwatch.ElapsedMilliseconds < 1000, "SaveChangesAsync took too long.");
    //}
}