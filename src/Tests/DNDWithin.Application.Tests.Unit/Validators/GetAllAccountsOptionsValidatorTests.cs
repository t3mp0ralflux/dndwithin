using Bogus;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Validators.Accounts;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Specialized;
using FluentValidation;
using FluentValidation.Results;
using ValidationException = FluentValidation.ValidationException;

namespace DNDWithin.Application.Tests.Unit.Validators;

public class GetAllAccountsOptionsValidatorTests
{
    public GetAllAccountsOptionsValidator _sut;

    public GetAllAccountsOptionsValidatorTests()
    {
        _sut = new GetAllAccountsOptionsValidator();
    }

    [Fact]
    public async Task Validator_ShouldThrowAsync_WhenSearchFieldIsInvalid()
    {
        // Arrange
        GetAllAccountsOptions options = new()
                                        {
                                            SortField = "bacon",
                                            Page = 1,
                                            PageSize = 10
                                        };

        // Act
        Func<Task> action = async () => await _sut.ValidateAndThrowAsync(options);

        // Assert
        ExceptionAssertions<ValidationException>? result = await action.Should().ThrowAsync<ValidationException>();

        AndWhichConstraint<GenericCollectionAssertions<ValidationFailure>, ValidationFailure>? errorList = result.Subject.FirstOrDefault()?.Errors.Should().ContainSingle();
        ValidationFailure? error = result.Subject.First().Errors.First();
        error.PropertyName.Should().Be("SortField");
        error.ErrorMessage.Should().Be("You can only sort by Username or Lastlogin");
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    public async Task Validator_ShouldThrowAsync_WhenPageValueIsInvalid(int page)
    {
        // Arrange
        GetAllAccountsOptions options = new()
                                        {
                                            SortField = "username",
                                            Page = page,
                                            PageSize = 10
                                        };

        // Act
        Func<Task> action = async () => await _sut.ValidateAndThrowAsync(options);

        // Assert
        ExceptionAssertions<ValidationException>? result = await action.Should().ThrowAsync<ValidationException>();

        AndWhichConstraint<GenericCollectionAssertions<ValidationFailure>, ValidationFailure>? errorList = result.Subject.FirstOrDefault()?.Errors.Should().ContainSingle();
        ValidationFailure? error = result.Subject.First().Errors.First();
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
        GetAllAccountsOptions options = new()
                                        {
                                            SortField = "username",
                                            Page = 1,
                                            PageSize = pageSize
                                        };

        // Act
        Func<Task> action = async () => await _sut.ValidateAndThrowAsync(options);

        // Assert
        ExceptionAssertions<ValidationException>? result = await action.Should().ThrowAsync<ValidationException>();

        AndWhichConstraint<GenericCollectionAssertions<ValidationFailure>, ValidationFailure>? errorList = result.Subject.FirstOrDefault()?.Errors.Should().ContainSingle();
        ValidationFailure? error = result.Subject.First().Errors.First();
        error.PropertyName.Should().Be("PageSize");
    }

    [Fact]
    public async Task Validator_DoesNotThrowError_WhenValidationSucceeds()
    {
        // Arrange
        Faker<GetAllAccountsOptions>? options = new Faker<GetAllAccountsOptions>()
                                                .RuleFor(x => x.AccountRole, f => AccountRole.standard)
                                                .RuleFor(x => x.AccountStatus, f => AccountStatus.active)
                                                .RuleFor(x => x.SortField, "username")
                                                .RuleFor(x => x.Page, f => f.Random.Int(1, 100))
                                                .RuleFor(x => x.PageSize, f => f.Random.Int(1, 25));

        // Act
        Func<Task> action = async () => await _sut.ValidateAndThrowAsync(options);

        // Assert
        await action.Should().NotThrowAsync<ValidationException>();
    }
}