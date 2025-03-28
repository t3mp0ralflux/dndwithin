using DNDWithin.Api.Controllers;
using DNDWithin.Api.Services;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Services;
using DNDWithin.Application.Services.Implementation;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Testing.Common;

namespace DNDWithin.API.Tests.Unit;

public class AuthControllerTests
{
    private readonly IAccountService _accountService = Substitute.For<IAccountService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGeneratorService _jwtService = Substitute.For<IJwtTokenGeneratorService>();
    
    public AuthControllerTests()
    {
        _sut = new AuthController(_accountService, _passwordHasher, _jwtService);
    }
    public AuthController _sut { get; set; }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenEmailIsNotFound()
    {
        // Arrange
        var request = new LoginRequest()
                      {
                          Email = "test@test.test",
                          Password = "Bingus"
                      };

        _accountService.GetByEmailAsync(request.Email, CancellationToken.None).Returns((Account?)null);
        
        // Act
        var result = (NotFoundResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenUsernameIsNotFound()
    {
        // Arrange
        var request = new LoginRequest()
                      {
                          Email = "Dingus",
                          Password = "Bingus"
                      };

        _accountService.GetByUsernameAsync(request.Email, CancellationToken.None).Returns((Account?)null);
        // Act
        var result = (NotFoundResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenEmailIsCorrectAndPasswordIsIncorrect()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var request = new LoginRequest()
                      {
                          Email = account.Email,
                          Password = account.Password
                      };

        _accountService.GetByEmailAsync(request.Email, CancellationToken.None).Returns(account);

        _passwordHasher.Verify(request.Password, account.Password).Returns(false);

        // Act
        var result = (UnauthorizedResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(401);
    }
    
    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUsernameIsCorrectAndPasswordIsIncorrect()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var request = new LoginRequest()
                      {
                          Email = account.Username,
                          Password = account.Password
                      };

        _accountService.GetByUsernameAsync(request.Email, CancellationToken.None).Returns(account);

        _passwordHasher.Verify(request.Password, account.Password).Returns(false);

        // Act
        var result = (UnauthorizedResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_ShouldReturnJwtToken_WhenDataIsCorrect()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var request = new LoginRequest()
                      {
                          Email = account.Email,
                          Password = account.Password
                      };

        var expectedToken = "ThisIsTheSingleBestTokenYouHaveEverSeen";
        
        _accountService.GetByEmailAsync(request.Email, CancellationToken.None).Returns(account);
        _passwordHasher.Verify(request.Password, account.Password).Returns(true);
        _jwtService.GenerateToken(account, request, CancellationToken.None).Returns(expectedToken);
        
        // Act
        var result = (OkObjectResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().Be(expectedToken);
    }
}