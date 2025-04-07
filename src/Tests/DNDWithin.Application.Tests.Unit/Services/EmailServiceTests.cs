using DNDWithin.Application.Models.System;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Services.Implementation;
using FluentAssertions;
using NSubstitute;
using Testing.Common;

namespace DNDWithin.Application.Tests.Unit.Services;

public class EmailServiceTests
{
    public EmailServiceTests()
    {
        _sut = new EmailService(_emailRepository, _dateTimeProvider);
    }

    public EmailService _sut { get; set; }
    private readonly IEmailRepository _emailRepository = Substitute.For<IEmailRepository>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    [Fact]
    public async Task QueueEmail_ShouldQueueEmail_WhenMethodIsCalled()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var emailData = Fakes.GenerateEmailData(now);
        
        // Act
        await _sut.QueueEmailAsync(emailData);

        // Assert
        var repositoryCall = _emailRepository.ReceivedCalls().FirstOrDefault();
        repositoryCall.Should().NotBeNull();

        var sentData = (EmailData?)repositoryCall.GetArguments().FirstOrDefault();
        sentData.Should().NotBeNull();
        sentData.Should().BeEquivalentTo(emailData, options => options.Excluding(x => x.Id).Excluding(x => x.ResponseLog));

        sentData.ResponseLog.Should().Contain("Email Queued;");
    }

    [Fact]
    public async Task GetForProcessingAsync_ShouldReturnEmptyList_WhenNothingIsAvailableForProcessing()
    {
        // Arrange
        _emailRepository.GetForProcessingAsync(Arg.Any<int>()).Returns([]);
        
        // Act
        var result = await _sut.GetForProcessingAsync(int.MaxValue);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetForProcessingAsync_ShouldReturnEmails_WhenItemsAreAvailable()
    {
        // Arrange
        List<EmailData> returnedItems = [Fakes.GenerateEmailData()];

        _emailRepository.GetForProcessingAsync(5).Returns(returnedItems);

        // Act
        var result = await _sut.GetForProcessingAsync(5);

        // Assert
        result.Should().BeEquivalentTo(returnedItems);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenUpdateFails()
    {
        // Arrange
        var emailData = Fakes.GenerateEmailData();

        _emailRepository.UpdateAsync(Arg.Any<EmailData>()).Returns(false);

        // Act
        var result = await _sut.UpdateAsync(emailData);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task UpdateAsync_ReturnsTrue_WhenUpdateSucceeds()
    {
        // Arrange
        var emailData = Fakes.GenerateEmailData();

        _emailRepository.UpdateAsync(Arg.Any<EmailData>()).Returns(true);

        // Act
        var result = await _sut.UpdateAsync(emailData);

        // Assert
        result.Should().BeTrue();
    }
}