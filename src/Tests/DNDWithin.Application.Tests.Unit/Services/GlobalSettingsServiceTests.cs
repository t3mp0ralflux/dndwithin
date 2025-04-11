using DNDWithin.Application.Models.GlobalSettings;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Services.Implementation;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Testing.Common;

namespace DNDWithin.Application.Tests.Unit.Services;

public class GlobalSettingsServiceTests
{
    private readonly IValidator<GetAllGlobalSettingsOptions> _globalSettingOptionsValidator = Substitute.For<IValidator<GetAllGlobalSettingsOptions>>();
    private readonly IGlobalSettingsRepository _globalSettingsRepository = Substitute.For<IGlobalSettingsRepository>();
    private readonly IValidator<GlobalSetting> _globalSettingValidator = Substitute.For<IValidator<GlobalSetting>>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IMemoryCache _memoryCache = Substitute.For<IMemoryCache>();

    public GlobalSettingsServiceTests()
    {
        _sut = new GlobalSettingsService(_globalSettingsRepository, _globalSettingValidator, _globalSettingOptionsValidator, _dateTimeProvider, _memoryCache);
    }

    public GlobalSettingsService _sut { get; set; }

    [Fact]
    public async Task CreateSetting_ReturnsFalse_WhenDbFailsToCreateSetting()
    {
        // Arrange
        GlobalSetting setting = new()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "",
                                    Value = ""
                                };

        _globalSettingsRepository.CreateSetting(Arg.Any<GlobalSetting>()).Returns(false);

        // Act
        bool result = await _sut.CreateSettingAsync(setting);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateSetting_ReturnsTrue_WhenSettingIsCreated()
    {
        // Arrange
        GlobalSetting setting = new()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "",
                                    Value = ""
                                };

        _globalSettingsRepository.CreateSetting(Arg.Any<GlobalSetting>()).Returns(true);

        // Act
        bool result = await _sut.CreateSettingAsync(setting);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoSettingsAreFound()
    {
        // Arrange
        GetAllGlobalSettingsOptions options = new()
                                              {
                                                  Page = 1,
                                                  PageSize = 5
                                              };
        _globalSettingsRepository.GetAllAsync(options).Returns([]);

        // Act
        List<GlobalSetting> result = (await _sut.GetAllAsync(options)).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSettingList_WhenSettingsAreFound()
    {
        // Arrange
        GetAllGlobalSettingsOptions options = new()
                                              {
                                                  Page = 1,
                                                  PageSize = 5
                                              };

        GlobalSetting settings = Fakes.GenerateGlobalSetting();

        _globalSettingsRepository.GetAllAsync(options).Returns([settings]);

        // Act
        List<GlobalSetting> result = (await _sut.GetAllAsync(options)).ToList();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCount_WhenDbIsQueried()
    {
        // Arrange
        GlobalSetting setting = Fakes.GenerateGlobalSetting();

        _globalSettingsRepository.GetCountAsync(setting.Name).Returns(1);

        // Act
        int result = await _sut.GetCountAsync(setting.Name);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetSettingAsync_ShouldReturnNull_WhenSettingIsNotFound()
    {
        // Arrange
        var setting = Fakes.GenerateGlobalSetting();

        _globalSettingsRepository.GetSetting(setting.Name).Returns((GlobalSetting?)null);

        // Act
        var result = await _sut.GetSettingAsync(setting.Name);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSettingAsync_ShouldReturnSetting_WhenSettingIsFound()
    {
        // Arrange
        var setting = Fakes.GenerateGlobalSetting();

        _globalSettingsRepository.GetSetting(setting.Name).Returns(setting);

        // Act
        var result = await _sut.GetSettingAsync(setting.Name);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(setting, options => options.Excluding(x => x.Id));
    }

    [Theory]
    [InlineData("Test", typeof(string))]
    [InlineData(1, typeof(int))]
    [InlineData(true, typeof(bool))]
    [InlineData("2015-01-01T00:00:00", typeof(DateTime))]
    [InlineData("420.69", typeof(double))]
    public async Task GetSettingAsync_ShouldReturnDefaultValue_WhenSettingIsNotFound(object value, Type valueType)
    {
        // Arrange
        var setting = Fakes.GenerateGlobalSetting(value.ToString());

        _globalSettingsRepository.GetSetting(setting.Name).Returns((GlobalSetting?)null);

        bool boolResult = true;
        int intResult = int.MinValue;
        double doubleResult = double.MinValue;
        DateTime dateTimeResult = DateTime.MinValue;
        string stringResult = "WhatKindOfStringIsThis";

        // Act
        switch (Type.GetTypeCode(valueType))
        {
            case TypeCode.Boolean:
                boolResult = await _sut.GetSettingAsync(setting.Name, false);
                break;
            case TypeCode.Int16:
            case TypeCode.Int32:
                intResult = await _sut.GetSettingAsync(setting.Name, int.MaxValue);
                break;
            case TypeCode.Double:
                doubleResult = await _sut.GetSettingAsync(setting.Name, double.MaxValue);
                break;
            case TypeCode.DateTime:
                dateTimeResult = await _sut.GetSettingAsync(setting.Name, DateTime.UtcNow);
                break;
            case TypeCode.String:
                stringResult = await _sut.GetSettingAsync(setting.Name, "Default String");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // Assert
        switch (Type.GetTypeCode(valueType))
        {
            case TypeCode.Boolean:
                boolResult.Should().BeFalse();
                break;
            case TypeCode.Int16:
            case TypeCode.Int32:
                intResult.Should().Be(int.MaxValue);
                break;
            case TypeCode.Double:
                doubleResult.Should().Be(double.MaxValue);
                break;
            case TypeCode.DateTime:
                dateTimeResult.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
                break;
            case TypeCode.String:
                stringResult.Should().NotBeNull();
                stringResult.Should().Be("Default String");
                break;
            default:
                Assert.Fail("Wrong conversion type");
                break;
        }
    }

    [Theory]
    [InlineData(1, typeof(DateTime))]
    [InlineData("Big String", typeof(int))]
    [InlineData("420.49", typeof(bool))]
    [InlineData(false, typeof(DateTime))]
    [InlineData("2025-01-01T00:00:00", typeof(double))]
    public async Task GetSettingAsync_ShouldReturnDefaultValue_WhenSettingHasIncorrectValue(object value, Type valueType)
    {
        // Arrange
        var setting = Fakes.GenerateGlobalSetting(value.ToString());

        _globalSettingsRepository.GetSetting(setting.Name).Returns(setting);

        bool boolResult = true;
        int intResult = int.MinValue;
        double doubleResult = double.MinValue;
        DateTime dateTimeResult = DateTime.MinValue;
        string stringResult = "WhatKindOfStringIsThis";

        // Act
        switch (Type.GetTypeCode(valueType))
        {
            case TypeCode.Boolean:
                boolResult = await _sut.GetSettingAsync(setting.Name, false);
                break;
            case TypeCode.Int16:
            case TypeCode.Int32:
                intResult = await _sut.GetSettingAsync(setting.Name, int.MaxValue);
                break;
            case TypeCode.Double:
                doubleResult = await _sut.GetSettingAsync(setting.Name, double.MaxValue);
                break;
            case TypeCode.DateTime:
                dateTimeResult = await _sut.GetSettingAsync(setting.Name, DateTime.UtcNow);
                break;
            case TypeCode.String:
                stringResult = await _sut.GetSettingAsync(setting.Name, "Default String");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // Assert
        switch (Type.GetTypeCode(valueType))
        {
            case TypeCode.Boolean:
                boolResult.Should().BeFalse();
                break;
            case TypeCode.Int16:
            case TypeCode.Int32:
                intResult.Should().Be(int.MaxValue);
                break;
            case TypeCode.Double:
                doubleResult.Should().Be(double.MaxValue);
                break;
            case TypeCode.DateTime:
                dateTimeResult.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
                break;
            case TypeCode.String:
                stringResult.Should().NotBeNull();
                stringResult.Should().Be("Default String");
                break;
            default:
                Assert.Fail("Wrong conversion type");
                break;
        }
    }
    
    [Theory]
    [InlineData("Test", typeof(string))]
    [InlineData(1, typeof(int))]
    [InlineData(true, typeof(bool))]
    [InlineData("2015-01-01T00:00:00", typeof(DateTime))]
    [InlineData("420.69", typeof(double))]
    public async Task GetSettingAsync_ShouldReturnValue_WhenSettingIsFoundAndHasCorrectValue(object value, Type valueType)
    {
        // Arrange
        var setting = Fakes.GenerateGlobalSetting(value.ToString());

        _globalSettingsRepository.GetSetting(setting.Name).Returns(setting);

        bool boolResult = true;
        int intResult = int.MinValue;
        double doubleResult = double.MinValue;
        DateTime dateTimeResult = DateTime.MinValue;
        string stringResult = "WhatKindOfStringIsThis";

        // Act
        switch (Type.GetTypeCode(valueType))
        {
            case TypeCode.Boolean:
                boolResult = await _sut.GetSettingAsync(setting.Name, false);
                break;
            case TypeCode.Int16:
            case TypeCode.Int32:
                intResult = await _sut.GetSettingAsync(setting.Name, int.MaxValue);
                break;
            case TypeCode.Double:
                doubleResult = await _sut.GetSettingAsync(setting.Name, double.MaxValue);
                break;
            case TypeCode.DateTime:
                dateTimeResult = await _sut.GetSettingAsync(setting.Name, DateTime.UtcNow);
                break;
            case TypeCode.String:
                stringResult = await _sut.GetSettingAsync(setting.Name, "Default String");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // Assert
        switch (Type.GetTypeCode(valueType))
        {
            case TypeCode.Boolean:
                boolResult.Should().Be((bool)value);
                break;
            case TypeCode.Int16:
            case TypeCode.Int32:
                intResult.Should().Be((int)value);
                break;
            case TypeCode.Double:
                doubleResult.Should().Be(double.Parse(value.ToString()!));
                break;
            case TypeCode.DateTime:
                dateTimeResult.Should().BeCloseTo(DateTime.Parse(value.ToString()!), TimeSpan.FromSeconds(1));
                break;
            case TypeCode.String:
                stringResult.Should().NotBeNull();
                stringResult.Should().Be(value.ToString());
                break;
            default:
                Assert.Fail("Wrong conversion type");
                break;
        }
    }
}