using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Validators.Accounts;
using FluentAssertions;
using FluentAssertions.Specialized;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Testing.Common;

namespace DNDWithin.Application.Tests.Unit.Validators;

public class AccountValidatorTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    public AccountValidator _sut;

    public AccountValidatorTests()
    {
        _sut = new AccountValidator(_accountRepository);
    }

    [Fact]
    public async Task Validator_ThrowsError_WhenFieldsAreMissing()
    {
        // Arrange
        Account account = new()
                          {
                              Id = Guid.NewGuid(),
                              FirstName = "",
                              LastName = "",
                              Username = "",
                              Email = "",
                              Password = ""
                          };

        List<string> expectedProperties = ["Email", "FirstName", "LastName", "Password", "Username"];

        // Act
        Func<Task> action = async () => await _sut.ValidateAndThrowAsync(account);

        // Assert
        ExceptionAssertions<ValidationException>? result = await action.Should().ThrowAsync<ValidationException>();

        IOrderedEnumerable<string>? errorList = result.Subject.FirstOrDefault()?.Errors.Select(x => x.PropertyName).Distinct().Order();

        errorList.Should().BeEquivalentTo(expectedProperties);
    }

    [Fact]
    public async Task Validator_ThrowsError_WhenEmailIsAlreadyInUse()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();

        _accountRepository.ExistsByEmailAsync(account.Email!).Returns(true);

        // Act
        Func<Task> action = async () => await _sut.ValidateAndThrowAsync(account);

        // Assert
        ExceptionAssertions<ValidationException>? result = await action.Should().ThrowAsync<ValidationException>();

        List<ValidationFailure>? errors = result.Subject.FirstOrDefault()?.Errors.ToList();
        errors.Should().ContainSingle();

        ValidationFailure error = errors.First();
        error.PropertyName.Should().Be("Email");
        error.ErrorMessage.Should().Be("Email already in use. Please login instead");
    }

    [Fact]
    public async Task Validator_ThrowsError_WhenUsernameIsAlreadyInUse()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();

        _accountRepository.ExistsByUsernameAsync(account.Username!).Returns(true);

        // Act
        Func<Task> action = async () => await _sut.ValidateAndThrowAsync(account);

        // Assert
        ExceptionAssertions<ValidationException>? result = await action.Should().ThrowAsync<ValidationException>();

        List<ValidationFailure>? errors = result.Subject.FirstOrDefault()?.Errors.ToList();
        errors.Should().ContainSingle();

        ValidationFailure error = errors.First();
        error.PropertyName.Should().Be("Username");
        error.ErrorMessage.Should().Be("Username already in use");
    }

    [Fact]
    public async Task Validator_DoesNotThrowError_WhenValidationSucceeds()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        _accountRepository.ExistsByEmailAsync(account.Email!).Returns(false);
        _accountRepository.ExistsByUsernameAsync(account.Username!).Returns(false);

        // Act
        Func<Task> action = async () => await _sut.ValidateAndThrowAsync(account);

        // Assert
        await action.Should().NotThrowAsync();
    }
}