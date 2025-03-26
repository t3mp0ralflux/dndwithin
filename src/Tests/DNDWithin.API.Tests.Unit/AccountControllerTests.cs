using Bogus;
using DNDWithin.Api.Controllers;
using DNDWithin.Api.Mapping;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Services;
using DNDWithin.Contracts.Requests.Account;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ValidationException = FluentValidation.ValidationException;

namespace DNDWithin.API.Tests.Unit;

public class AccountControllerTests
{
    public AccountControllerTests()
    {
        _sut = new AccountController(_AccountService);
    }

    public AccountController _sut { get; set; }
    public IAccountService _AccountService = Substitute.For<IAccountService>();
    public Faker _faker = new ();

    [Fact]
    public async Task Create_ShouldThrowBadRequest_WhenRequestIsMissingRequiredInformation()
    {
        // Arrange
        _AccountService.CreateAsync(Arg.Any<Account>()).Throws(new ValidationException("Information is required"));
        var request = new AccountCreateRequest()
                      {
                          Email = _faker.Person.Email,
                          FirstName = _faker.Person.FirstName,
                          LastName = _faker.Person.LastName,
                          Password = _faker.Internet.Password(),
                          UserName = _faker.Internet.UserName()
                      };
        // Act
        var result = async () => (BadRequestResult)await _sut.Create(request, CancellationToken.None);

        // Assert
        await result.Should().ThrowAsync<ValidationException>("Information is required");
    }

    [Fact]
    public async Task Create_ShouldCreateAccount_WhenRequestInformationIsPresent()
    {
        // Arrange
        _AccountService.CreateAsync(Arg.Any<Account>()).Returns(true);
        var request = new AccountCreateRequest()
                      {
                          Email = _faker.Person.Email,
                          FirstName = _faker.Person.FirstName,
                          LastName = _faker.Person.LastName,
                          Password = _faker.Internet.Password(),
                          UserName = _faker.Internet.UserName()
                      };

        var expectedResponse = request.ToAccount().ToResponse();
        
        // Act
        var result = (CreatedAtActionResult)await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.StatusCode.Should().Be(201);
        result.ActionName.Should().Be(nameof(_sut.Get));
        result.RouteValues.Should().NotBeEmpty();
        result.Value.Should().BeEquivalentTo(expectedResponse, options => options.Excluding(y => y.Id));
    }

    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenAccountIsNotFound()
    {
        // Arrange
        _AccountService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Account?)null);
        
        // Act
        var result = (NotFoundResult)await _sut.Get(Guid.NewGuid(), CancellationToken.None);
        
        // Assert
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Get_ShouldReturnAccount_WhenAccountIsFound()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        
        var account = new Account()
                      {
                          Id = accountId,
                          FirstName = _faker.Person.FirstName,
                          LastName = _faker.Person.LastName,
                          Email = _faker.Person.Email,
                          UserName = _faker.Internet.UserName(),
                          Password = _faker.Internet.Password(),
                          AccountRole = AccountRole.admin,
                          AccountStatus = AccountStatus.active,
                          CreatedUtc = DateTime.UtcNow,
                          UpdatedUtc = DateTime.UtcNow,
                          LastLoginUtc = DateTime.UtcNow,
                      };

        _AccountService.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(account);
        
        var expectedResponse = account.ToResponse();
        // Act
        var result = (OkObjectResult)await _sut.Get(accountId, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(expectedResponse);

    }
}