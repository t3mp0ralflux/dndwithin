using DNDWithin.Application.Models.Auth;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Services;
using DNDWithin.Application.Services.Implementation;
using FluentAssertions;
using NSubstitute;

namespace DNDWithin.Application.Tests.Unit.Services;

public class AuthServiceTests
{
    public IAuthService _sut { get; set; }
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();

    public AuthServiceTests()
    {
        _sut = new AuthService(_accountRepository);
    }

    [Fact]
    public async Task Login_ShouldReturnFalse_WhenLoginFails()
    {
        // Arrange
        var accountLogin = new AccountLogin()
                           {
                               Email = "test@test.com"
                           };
        
        _accountRepository.LoginAsync(Arg.Any<AccountLogin>()).Returns(false);
        
        // Act
        var result = await _sut.LoginAsync(accountLogin);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task Login_ShouldReturnTrue_WhenLoginSucceeds()
    {
        // Arrange
        var accountLogin = new AccountLogin()
                           {
                               Email = "test@test.com"
                           };
        
        _accountRepository.LoginAsync(Arg.Any<AccountLogin>()).Returns(true);
        
        // Act
        var result = await _sut.LoginAsync(accountLogin);

        // Assert
        result.Should().BeTrue();
    }
}