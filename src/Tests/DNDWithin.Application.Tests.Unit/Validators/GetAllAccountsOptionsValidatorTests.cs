using Bogus;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Validators.Accounts;
using FluentAssertions;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;

namespace DNDWithin.Application.Tests.Unit.Validators;

public class GetAllAccountsOptionsValidatorTests
{
    public GetAllAccountsOptionsValidatorTests()
    {
        _sut = new GetAllAccountsOptionsValidator();
    }
    
    public GetAllAccountsOptionsValidator _sut;

    [Fact]
    public async Task Validator_ShouldThrowAsync_WhenSearchFieldIsInvalid()
    {
        // Arrange
        var options = new GetAllAccountsOptions()
                      {
                          SortField = "bacon",
                          Page = 1,
                          PageSize = 10,
                      };
        
        // Act
        var action = async () => await _sut.ValidateAndThrowAsync(options);

        // Assert
        var result = await action.Should().ThrowAsync<ValidationException>();

        var errorList = result.Subject.FirstOrDefault()?.Errors.Should().ContainSingle();
        var error = result.Subject.First().Errors.First();
        error.PropertyName.Should().Be("SortField");
        error.ErrorMessage.Should().Be("You can only sort by Username or Lastlogin");
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    public async Task Validator_ShouldThrowAsync_WhenPageValueIsInvalid(int page)
    {
        // Arrange
        var options = new GetAllAccountsOptions()
                      {
                          SortField = "username",
                          Page = page,
                          PageSize = 10,
                      };
        
        // Act
        var action = async () => await _sut.ValidateAndThrowAsync(options);

        // Assert
        var result = await action.Should().ThrowAsync<ValidationException>();

        var errorList = result.Subject.FirstOrDefault()?.Errors.Should().ContainSingle();
        var error = result.Subject.First().Errors.First();
        error.PropertyName.Should().Be("Page");
    }
    
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(26)]
    [InlineData(int.MaxValue)]
    public async Task Validator_ShouldThrowAsync_WhenPageSizeValueIsInvalid(int pageSize)
    {
        // Arrange
        var options = new GetAllAccountsOptions()
                      {
                          SortField = "username",
                          Page = 1,
                          PageSize = pageSize,
                      };
        
        // Act
        var action = async () => await _sut.ValidateAndThrowAsync(options);

        // Assert
        var result = await action.Should().ThrowAsync<ValidationException>();

        var errorList = result.Subject.FirstOrDefault()?.Errors.Should().ContainSingle();
        var error = result.Subject.First().Errors.First();
        error.PropertyName.Should().Be("PageSize");
    }

    [Fact]
    public async Task Validator_DoesNotThrowError_WhenValidationSucceeds()
    {
        // Arrange
        var options = new Faker<GetAllAccountsOptions>()
            .RuleFor(x=>x.AccountRole, f=> AccountRole.standard)
            .RuleFor(x=>x.AccountStatus, f => AccountStatus.active)
            .RuleFor(x=>x.SortField, "username")
            .RuleFor(x=>x.Page, f=>f.Random.Int(1, 100))
            .RuleFor(x=>x.PageSize, f=>f.Random.Int(1, 25));

        // Act
        var action = async () => await _sut.ValidateAndThrowAsync(options);

        // Assert
        await action.Should().NotThrowAsync<ValidationException>();
    }
}