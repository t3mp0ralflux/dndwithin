using DNDWithin.Api.Controllers;
using DNDWithin.Api.Mapping;
using DNDWithin.Application.Models.GlobalSettings;
using DNDWithin.Application.Services;
using DNDWithin.Contracts.Requests.GlobalSetting;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DNDWithin.API.Tests.Unit;

public class GlobalSettingControllerTests
{
    public GlobalSettingControllerTests()
    {
        _sut = new GlobalSettingController(_globalSettingsService);
    }

    public GlobalSettingController _sut;
    private readonly IGlobalSettingsService _globalSettingsService = Substitute.For<IGlobalSettingsService>();

    [Fact]
    public async Task Create_ReturnsValidatorError_WhenValidationFails()
    {
        // Arrange
        var request = new GlobalSettingCreateRequest()
                      {
                          Name = "Test",
                          Value = "false"
                      };

        _globalSettingsService.CreateSettingAsync(Arg.Any<GlobalSetting>()).Throws(new ValidationException("Validation Failed"));

        // Act
        var result = async () => await _sut.Create(request, CancellationToken.None);

        // Assert
        await result.Should().ThrowAsync<ValidationException>("Validation Failed");
    }

    [Fact]
    public async Task Create_ReturnsGlobalSetting_WhenValidationPasses()
    {
        // Arrange
        var request = new GlobalSettingCreateRequest()
                      {
                          Name = "Test",
                          Value = "false"
                      };
        _globalSettingsService.CreateSettingAsync(Arg.Any<GlobalSetting>()).Returns(true);

        // Act
        var result = (CreatedAtActionResult)await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(201);
        result.Value.Should().BeEquivalentTo(request.ToGlobalSetting(), options => options.Excluding(x => x.Id));
    }
    
}