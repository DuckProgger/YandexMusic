using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Yandex.Api;
using Yandex.Music.Core.Cache;
using Yandex.Music.Core.Cache.Database;

namespace Yandex.Tests.Core.Cache;

[TestClass]
public class SqliteCacheTests
{
    private readonly SqliteCacheProvider sqliteCacheProvider;
    // private readonly Mock<ILogger<SqliteCacheProvider>> logger = new();

    public SqliteCacheTests() {
        sqliteCacheProvider = new(new SqliteCacheProviderSettings() {
            DatabasePath = "test.db3",
        });

        // Пересоздать БД
        using SqliteCacheDatabaseContext Context = sqliteCacheProvider.DbContextPool.CreateDbContext();
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }

    [TestMethod]
    public async Task SqliteCacheTest_WhenGetExistsByteData_ShouldReturnByteData() {
        // Arrange
        byte[] data = { 1, 2, 3 };
        RequestData requestData = new() {
            RequestUrl = nameof(SqliteCacheTest_WhenGetExistsByteData_ShouldReturnByteData)
        };

        // Act
        await sqliteCacheProvider.SetBytesAsync(requestData, data, default);
        byte[] receivedData = await sqliteCacheProvider.GetBytesAsync(requestData, default);

        // Assert		
        CollectionAssert.AreEqual(data, receivedData);
    }

    [TestMethod]
    public async Task SqliteCacheTest_WhenGetNotExistsByteData_ShouldReturnNull() {
        // Arrange

        // Act
        byte[] receivedData = await sqliteCacheProvider.GetBytesAsync(new RequestData(), default);

        // Assert		
        Assert.IsNull(receivedData);
    }

    [TestMethod]
    public async Task SqliteCacheTest_WhenEmptyByteContent_ShouldThrowInvalidOperationException() {
        // Arrange
        byte[] data = { };
        RequestData requestData = new() {
            RequestUrl = nameof(SqliteCacheTest_WhenEmptyByteContent_ShouldThrowInvalidOperationException)
        };

        // Act
        Task act() {
            return sqliteCacheProvider.SetBytesAsync(requestData, data, default);
        }

        // Assert		
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(act);
    }

    [TestMethod]
    public async Task SqliteCacheTest_WhenNullByteContent_ShouldThrowInvalidOperationException() {
        // Arrange
        byte[] data = null;
        RequestData requestData = new() {
            RequestUrl = nameof(SqliteCacheTest_WhenNullByteContent_ShouldThrowInvalidOperationException)
        };

        // Act
        Task act() {
            return sqliteCacheProvider.SetBytesAsync(requestData, data, default);
        }

        // Assert		
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(act);
    }

    [TestMethod]
    public async Task SqliteCacheTest_WhenGetExistsStringData_ShouldReturnStringData() {
        // Arrange
        string data = "123";
        RequestData requestData = new() {
            RequestUrl = nameof(SqliteCacheTest_WhenGetExistsStringData_ShouldReturnStringData)
        };

        // Act
        await sqliteCacheProvider.SetStringAsync(requestData, data, default);
        string receivedData = await sqliteCacheProvider.GetStringAsync(requestData, default);

        // Assert		
        Assert.AreEqual(data, receivedData);
    }

    [TestMethod]
    public async Task SqliteCacheTest_WhenGetNotExistsStringData_ShouldReturnNull() {
        // Arrange

        // Act
        string receivedData = await sqliteCacheProvider.GetStringAsync(new RequestData(), default);

        // Assert		
        Assert.IsNull(receivedData);
    }

    [TestMethod]
    public async Task SqliteCacheTest_WhenEmptyStringContent_ShouldThrowInvalidOperationException() {
        // Arrange
        string data = "";
        RequestData requestData = new() {
            RequestUrl = nameof(SqliteCacheTest_WhenEmptyStringContent_ShouldThrowInvalidOperationException)
        };

        // Act
        Task act() {
            return sqliteCacheProvider.SetStringAsync(requestData, data, default);
        }

        // Assert		
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(act);
    }

    [TestMethod]
    public async Task SqliteCacheTest_WhenNullStringContent_ShouldThrowInvalidOperationException() {
        // Arrange
        string data = null;
        RequestData requestData = new() {
            RequestUrl = nameof(SqliteCacheTest_WhenNullStringContent_ShouldThrowInvalidOperationException)
        };

        // Act
        Task act() {
            return sqliteCacheProvider.SetStringAsync(requestData, data, default);
        }

        // Assert		
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(act);
    }
}
