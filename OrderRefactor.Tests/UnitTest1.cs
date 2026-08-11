using Xunit;
using QuotesApi.Services;

namespace OrderRefactor.Tests;

public class UnitTest1
{
    [Fact]
    public void FakeClock_Returns_Custom_Time()
    {
        // Arrange
        var expectedTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var fakeClock = new FakeClock(expectedTime);

        // Act
        var currentTime = fakeClock.UtcNow;

        // Assert
        Assert.Equal(expectedTime, currentTime);
    }
}

public class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }

    public FakeClock(DateTimeOffset initialTime)
    {
        UtcNow = initialTime;
    }
}
