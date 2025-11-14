using EasyBuy.Application.Contracts.Caching;
using EasyBuy.Infrastructure.Services.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyBuy.Application.UnitTests.Services;

public class LayeredCacheServiceTests
{
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly Mock<ILogger<LayeredCacheService>> _loggerMock;
    private readonly LayeredCacheService _service;

    public LayeredCacheServiceTests()
    {
        _memoryCacheMock = new Mock<IMemoryCache>();
        _distributedCacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<LayeredCacheService>>();

        _service = new LayeredCacheService(
            _memoryCacheMock.Object,
            _distributedCacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_ValueInL1Cache_ShouldReturnFromL1()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";
        object cacheEntry = expectedValue;

        _memoryCacheMock
            .Setup(x => x.TryGetValue(key, out cacheEntry))
            .Returns(true);

        // Act
        var result = await _service.GetAsync<string>(key);

        // Assert
        result.Should().Be(expectedValue);
        _distributedCacheMock.Verify(
            x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SetAsync_ShouldStoreInBothLayers()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMinutes(5);

        var memoryCacheEntryMock = new Mock<ICacheEntry>();
        memoryCacheEntryMock.SetupProperty(x => x.Value);
        memoryCacheEntryMock.SetupProperty(x => x.AbsoluteExpirationRelativeToNow);

        _memoryCacheMock
            .Setup(x => x.CreateEntry(key))
            .Returns(memoryCacheEntryMock.Object);

        // Act
        await _service.SetAsync(key, value, expiration);

        // Assert
        _memoryCacheMock.Verify(x => x.CreateEntry(key), Times.Once);
        _distributedCacheMock.Verify(
            x => x.SetAsync(
                key,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveFromBothLayers()
    {
        // Arrange
        var key = "test-key";

        // Act
        await _service.RemoveAsync(key);

        // Assert
        _memoryCacheMock.Verify(x => x.Remove(key), Times.Once);
        _distributedCacheMock.Verify(
            x => x.RemoveAsync(key, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_ValueNotInCache_ShouldExecuteFactory()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";
        var factoryCalled = false;

        object? cacheEntry = null;
        _memoryCacheMock
            .Setup(x => x.TryGetValue(key, out cacheEntry))
            .Returns(false);

        _distributedCacheMock
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var memoryCacheEntryMock = new Mock<ICacheEntry>();
        memoryCacheEntryMock.SetupProperty(x => x.Value);
        memoryCacheEntryMock.SetupProperty(x => x.AbsoluteExpirationRelativeToNow);

        _memoryCacheMock
            .Setup(x => x.CreateEntry(key))
            .Returns(memoryCacheEntryMock.Object);

        // Act
        var result = await _service.GetOrCreateAsync(
            key,
            async () =>
            {
                factoryCalled = true;
                return await Task.FromResult(expectedValue);
            },
            TimeSpan.FromMinutes(5));

        // Assert
        result.Should().Be(expectedValue);
        factoryCalled.Should().BeTrue();
    }
}
