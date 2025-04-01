using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Validators.Accounts;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Testing.Common;

namespace DNDWithin.Application.Tests.Unit.Validators;

public class AccountValidatorTests
{
    public AccountValidatorTests()
    {
        _sut = new AccountValidator(_accountRepository);
    }

    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    public AccountValidator _sut;

    [Fact]
    public async Task Validator_ThrowsError_WhenFieldsAreMissing()
    {
        // Arrange
        var account = new Account()
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
        var action = async () => await _sut.ValidateAndThrowAsync(account);

        // Assert
        var result = await action.Should().ThrowAsync<ValidationException>();

        var errorList = result.Subject.FirstOrDefault()?.Errors.Select(x=>x.PropertyName).Distinct().Order();

        errorList.Should().BeEquivalentTo(expectedProperties);
    }

    [Fact]
    public async Task Validator_ThrowsError_WhenEmailIsAlreadyInUse()
    {
        // Arrange
        var account = Fakes.GenerateAccount();

        _accountRepository.ExistsByEmailAsync(account.Email!).Returns(account);
        
        // Act
        var action = async () => await _sut.ValidateAndThrowAsync(account);

        // Assert
        var result = await action.Should().ThrowAsync<ValidationException>();

        List<ValidationFailure>? errors = result.Subject.FirstOrDefault()?.Errors.ToList();
        errors.Should().ContainSingle();

        var error = errors.First();
        error.PropertyName.Should().Be("Email");
        error.ErrorMessage.Should().Be("Email already in use. Please login instead");
    }
    
    [Fact]
    public async Task Validator_ThrowsError_WhenUsernameIsAlreadyInUse()
    {
        // Arrange
        var account = Fakes.GenerateAccount();

        _accountRepository.ExistsByUsernameAsync(account.Username!).Returns(account);
        
        // Act
        var action = async () => await _sut.ValidateAndThrowAsync(account);

        // Assert
        var result = await action.Should().ThrowAsync<ValidationException>();

        List<ValidationFailure>? errors = result.Subject.FirstOrDefault()?.Errors.ToList();
        errors.Should().ContainSingle();

        var error = errors.First();
        error.PropertyName.Should().Be("Username");
        error.ErrorMessage.Should().Be("Username already in use");
    }

    [Fact]
    public async Task Validator_DoesNotThrowError_WhenValidationSucceeds()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        _accountRepository.ExistsByEmailAsync(account.Email!).Returns((Account?)null);
        _accountRepository.ExistsByUsernameAsync(account.Username!).Returns((Account?)null);

        // Act
        var action = async () => await _sut.ValidateAndThrowAsync(account);

        // Assert
        await action.Should().NotThrowAsync();
    }
}