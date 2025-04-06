using DNDWithin.Application.Services.Implementation;
using FluentAssertions;

namespace DNDWithin.Application.Tests.Unit.Services;

public class DateTimeProviderServiceTests
{
    public DateTimeProviderServiceTests()
    {
        _sut = new DateTimeProvider();
    }

    public DateTimeProvider _sut { get; set; }

    [Fact]
    public void GetDateTimeUtc_ShouldReturnUtcNow_WhenMethodIsCalled()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;

        // Act
        DateTime result = _sut.GetUtcNow();

        // Assert
        result.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }
}