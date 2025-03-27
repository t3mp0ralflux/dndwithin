﻿using Bogus;
using DNDWithin.Api.Controllers;
using DNDWithin.Api.Mapping;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Services;
using DNDWithin.Application.Tests.Integration;
using DNDWithin.Contracts.Requests.Account;
using DNDWithin.Contracts.Responses.Account;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Testing.Common;
using ValidationException = FluentValidation.ValidationException;

namespace DNDWithin.API.Tests.Unit;

public class AccountControllerTests
{
    public IAccountService _AccountService = Substitute.For<IAccountService>();

    public AccountControllerTests()
    {
        _sut = new AccountController(_AccountService);
    }

    public AccountController _sut { get; set; }

    [Fact]
    public async Task Create_ShouldThrowException_WhenRequestIsMissingRequiredInformation()
    {
        // Arrange
        _AccountService.CreateAsync(Arg.Any<Account>()).Throws(new ValidationException("Information is required"));
        var fakeAccount = Fakes.GenerateAccount();
        
        AccountCreateRequest request = new()
                                       {
                                           Email = fakeAccount.Email,
                                           FirstName = fakeAccount.FirstName,
                                           LastName = fakeAccount.LastName,
                                           Password = fakeAccount.Password,
                                           UserName = fakeAccount.Username
                                       };
        // Act
        Func<Task<BadRequestResult>> result = async () => (BadRequestResult)await _sut.Create(request, CancellationToken.None);

        // Assert
        await result.Should().ThrowAsync<ValidationException>("Information is required");
    }

    [Fact]
    public async Task Create_ShouldCreateAccount_WhenRequestInformationIsPresent()
    {
        // Arrange
        _AccountService.CreateAsync(Arg.Any<Account>()).Returns(true);
        var fakeAccount = Fakes.GenerateAccount();
        
        AccountCreateRequest request = new()
                                       {
                                           Email = fakeAccount.Email,
                                           FirstName = fakeAccount.FirstName,
                                           LastName = fakeAccount.LastName,
                                           Password = fakeAccount.Password,
                                           UserName = fakeAccount.Username
                                       };

        AccountResponse expectedResponse = request.ToAccount().ToResponse();

        // Act
        CreatedAtActionResult result = (CreatedAtActionResult)await _sut.Create(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
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
        NotFoundResult result = (NotFoundResult)await _sut.Get(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Get_ShouldReturnAccount_WhenAccountIsFound()
    {
        // Arrange
        Guid accountId = Guid.NewGuid();

        Account account = Fakes.GenerateAccount();

        _AccountService.GetByIdAsync(accountId, Arg.Any<CancellationToken>()).Returns(account);

        AccountResponse expectedResponse = account.ToResponse();
        // Act
        OkObjectResult result = (OkObjectResult)await _sut.Get(accountId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAll_ShouldThrowException_WhenWrongSortFieldsArePassedIn()
    {
        // Arrange
        GetAllAccountsRequest request = new()
                                        {
                                            Page = 1,
                                            PageSize = 5
                                        };

        _AccountService.GetAllAsync(Arg.Any<GetAllAccountsOptions>(), CancellationToken.None).Throws(new ValidationException("Bad Search Term"));
        // Act
        Func<Task<IActionResult>> action = async () => await _sut.GetAll(request, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ValidationException>("Bad Search Term");
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyResult_WhenNoItemsAreFound()
    {
        // Arrange
        GetAllAccountsRequest request = new()
                                        {
                                            UserName = "Test",
                                            Page = 1,
                                            PageSize = 5
                                        };

        AccountsResponse expectedResponse = new()
                                            {
                                                Items = [],
                                                Page = 1,
                                                PageSize = 5,
                                                Total = 0
                                            };

        // Act
        OkObjectResult result = (OkObjectResult)await _sut.GetAll(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetAll_ShouldReturnResults_WhenItemsAreFound()
    {
        // Arrange
        GetAllAccountsRequest request = new()
                                        {
                                            UserName = "Test",
                                            Page = 1,
                                            PageSize = 5
                                        };

        GetAllAccountsOptions requestOptions = request.ToOptions();

        Random random = new();

        List<Account> accounts = Enumerable.Range(5, random.Next(1, 15)).Select(x => Fakes.GenerateAccount()).ToList();

        _AccountService.GetAllAsync(Arg.Any<GetAllAccountsOptions>(), CancellationToken.None).Returns(accounts);
        _AccountService.GetCountAsync(requestOptions.UserName, CancellationToken.None).Returns(accounts.Count);

        AccountsResponse expectedResponse = accounts.ToResponse(request.Page, request.PageSize, accounts.Count());

        // Act
        OkObjectResult results = (OkObjectResult)await _sut.GetAll(request, CancellationToken.None);

        // Assert
        results.Should().NotBeNull();
        results.StatusCode.Should().Be(200);
        results.Value.Should().BeEquivalentTo(expectedResponse);
    }
}