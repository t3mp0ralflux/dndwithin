﻿using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Services;
using DNDWithin.Application.Services.Implementation;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using NSubstitute.Core;
using Testing.Common;

namespace DNDWithin.Application.Tests.Unit.Services;

public class AccountServiceTests
{
    private readonly IAccountRepository _accountRepository = Substitute.For<IAccountRepository>();
    private readonly IValidator<Account> _accountValidator = Substitute.For<IValidator<Account>>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IValidator<GetAllAccountsOptions> _optionsValidator = Substitute.For<IValidator<GetAllAccountsOptions>>();
    private readonly IPasswordHasher _passwordHasher = new PasswordHasher();

    public AccountServiceTests()
    {
        _sut = new AccountService(_accountRepository, _accountValidator, _dateTimeProvider, _optionsValidator, _passwordHasher);
    }

    public AccountService _sut { get; set; }

    [Fact]
    public async Task CreateAsync_ShouldInsertValidInformation_WhenAccountIsCreated()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        Account? account = Fakes.GenerateAccount();

        _accountRepository.CreateAsync(Arg.Any<Account>(), CancellationToken.None).Returns(true);
        _dateTimeProvider.GetUtcNow().Returns(now);

        // Act
        bool result = await _sut.CreateAsync(account.Clone());

        // Assert
        result.Should().BeTrue();
        ICall? createCall = _accountRepository.ReceivedCalls().FirstOrDefault();
        createCall.Should().NotBeNull();

        Account? createdAccount = (Account?)createCall.GetArguments().FirstOrDefault();
        createdAccount.Should().NotBeNull();

        _passwordHasher.Verify(account.Password, createdAccount.Password).Should().BeTrue();
        createdAccount.CreatedUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        createdAccount.UpdatedUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }
}