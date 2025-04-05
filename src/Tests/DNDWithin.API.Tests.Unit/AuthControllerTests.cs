using DNDWithin.Api.Controllers;
using DNDWithin.Api.Services;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Testing.Common;

namespace DNDWithin.API.Tests.Unit;

public class AuthControllerTests
{
    private readonly IAccountService _accountService = Substitute.For<IAccountService>();
    private readonly IJwtTokenGeneratorService _jwtService = Substitute.For<IJwtTokenGeneratorService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    public AuthControllerTests()
    {
        _sut = new AuthController(_accountService, _passwordHasher, _jwtService);
    }

    public AuthController _sut { get; set; }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenEmailIsNotFound()
    {
        // Arrange
        LoginRequest request = new()
                               {
                                   Email = "test@test.test",
                                   Password = "Bingus"
                               };

        _accountService.GetByEmailAsync(request.Email, CancellationToken.None).Returns((Account?)null);

        // Act
        NotFoundResult result = (NotFoundResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenUsernameIsNotFound()
    {
        // Arrange
        LoginRequest request = new()
                               {
                                   Email = "Dingus",
                                   Password = "Bingus"
                               };

        _accountService.GetByUsernameAsync(request.Email, CancellationToken.None).Returns((Account?)null);
        // Act
        NotFoundResult result = (NotFoundResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenEmailIsCorrectAndPasswordIsIncorrect()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        LoginRequest request = new()
                               {
                                   Email = account.Email,
                                   Password = account.Password
                               };

        _accountService.GetByEmailAsync(request.Email, CancellationToken.None).Returns(account);

        _passwordHasher.Verify(request.Password, account.Password).Returns(false);

        // Act
        UnauthorizedObjectResult result = (UnauthorizedObjectResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(401);
        result.Value.Should().Be("Username or password is incorrect");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUsernameIsCorrectAndPasswordIsIncorrect()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        LoginRequest request = new()
                               {
                                   Email = account.Username,
                                   Password = account.Password
                               };

        _accountService.GetByUsernameAsync(request.Email, CancellationToken.None).Returns(account);

        _passwordHasher.Verify(request.Password, account.Password).Returns(false);

        // Act
        UnauthorizedObjectResult result = (UnauthorizedObjectResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(401);
        result.Value.Should().Be("Username or password is incorrect");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenAccountIsNotActive()
    {
        // Arrange
        Account account = Fakes.GenerateAccount(AccountStatus.created);
        LoginRequest request = new()
                               {
                                   Email = account.Email,
                                   Password = account.Password
                               };

        _accountService.GetByEmailAsync(request.Email, CancellationToken.None).Returns(account);

        // Act
        UnauthorizedObjectResult result = (UnauthorizedObjectResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(401);
        result.Value.Should().Be("You must activate your account before you can login");
    }

    [Fact]
    public async Task Login_ShouldReturnJwtToken_WhenDataIsCorrect()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        LoginRequest request = new()
                               {
                                   Email = account.Email,
                                   Password = account.Password
                               };

        string expectedToken = "ThisIsTheSingleBestTokenYouHaveEverSeen";

        _accountService.GetByEmailAsync(request.Email, CancellationToken.None).Returns(account);
        _passwordHasher.Verify(request.Password, account.Password).Returns(true);
        _jwtService.GenerateToken(account, request, CancellationToken.None).Returns(expectedToken);

        // Act
        OkObjectResult result = (OkObjectResult)await _sut.Login(request, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().Be(expectedToken);
    }
}