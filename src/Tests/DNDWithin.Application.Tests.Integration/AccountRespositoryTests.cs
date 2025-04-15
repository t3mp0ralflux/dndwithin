using DNDWithin.Api.Mapping;
using DNDWithin.Application.Database;
using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Auth;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Repositories.Implementation;
using DNDWithin.Application.Services.Implementation;
using DNDWithin.Contracts.Requests.Account;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Testing.Common;
using ctr = DNDWithin.Contracts.Models;

namespace DNDWithin.Application.Tests.Integration;

public class AccountRespositoryTests : IClassFixture<ApplicationApiFactory>
{
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    public AccountRespositoryTests(ApplicationApiFactory apiFactory)
    {
        IDbConnectionFactory connectionFactory = apiFactory.Services.GetRequiredService<IDbConnectionFactory>();

        _sut = new AccountRepository(connectionFactory, _dateTimeProvider);
    }

    public IAccountRepository _sut { get; set; }


    [SkipIfEnvironmentMissingFact]
    public async Task CreateAsync_ShouldCreateAccount_WhenDataIsPassed()
    {
        // Arrange
        Account account = Fakes.GenerateAccount().WithActivation();

        // Act
        bool result = await _sut.CreateAsync(account);

        // Assert
        result.Should().BeTrue();
    }

    [SkipIfEnvironmentMissingTheory]
    [MemberData(nameof(GetSingleSearchUsernameData))]
    public async Task UsernameExistsAsync_ShouldReturnNull_WhenUsernameDoesNotExist(Account account, string username)
    {
        // Arrange
        account = account.WithActivation();

        await _sut.CreateAsync(account);
        await _sut.ActivateAsync(account);

        // Act
        Account? result = await _sut.GetByUsernameAsync("BigChungus");

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingTheory]
    [MemberData(nameof(GetSingleSearchUsernameData))]
    public async Task UsernameExistsAsync_ShouldReturnAccount_WhenUsernameExists(Account account, string username)
    {
        // Arrange
        account.ActivationCode = "Test";
        account.ActivationExpiration = DateTime.UtcNow;

        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsNotFound()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();

        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByIdAsync_ShouldReturnAccount_WhenIdIsFound()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();

        account.ActivationCode = "Test";
        account.ActivationExpiration = DateTime.UtcNow;

        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByIdAsync(account.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnvironmentMissingTheory]
    [InlineData(AccountStatus.banned, null, null)]
    [InlineData(null, AccountRole.standard, null)]
    [InlineData(null, null, "Bingus")]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNothingIsFound(AccountStatus? accountStatus, AccountRole? accountRole, string? userName)
    {
        // Arrange
        Account account = Fakes.GenerateAccount(); // defaults to Active, Admin

        GetAllAccountsOptions options = new()
                                        {
                                            AccountStatus = accountStatus,
                                            AccountRole = accountRole,
                                            UserName = userName,
                                            Page = 1,
                                            PageSize = 10
                                        };
        await _sut.CreateAsync(account);

        // Act
        IEnumerable<Account> result = await _sut.GetAllAsync(options);

        // Assert
        result.Should().BeEmpty();
    }

    [SkipIfEnvironmentMissingTheory]
    [MemberData(nameof(GetSingleSearchUsernameData))]
    public async Task GetAllAsync_ShouldReturnListWithOneAccount_WhenItemIsFound(Account accountToFind, string username)
    {
        // Arrange

        // defaults to Active, Admin
        List<Account> accounts = Enumerable.Range(5, 10).Select(x => Fakes.GenerateAccount()).ToList();

        DateTime now = DateTime.UtcNow;

        accountToFind.ActivationExpiration = now;
        accountToFind.ActivationCode = "Test";

        accounts.Add(accountToFind);

        foreach (Account account in accounts)
        {
            await _sut.CreateAsync(account);
        }

        List<Account> expectedResult = [accountToFind];

        GetAllAccountsOptions getAllOptions = new()
                                              {
                                                  AccountStatus = accountToFind.AccountStatus,
                                                  AccountRole = accountToFind.AccountRole,
                                                  UserName = username,
                                                  Page = 1,
                                                  PageSize = 5
                                              };
        // Act
        IEnumerable<Account> enumerableResult = await _sut.GetAllAsync(getAllOptions);
        List<Account> result = enumerableResult.ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(expectedResult, options => options
                                                                  .Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1)))
                                                                  .WhenTypeIs<DateTime>());
    }

    [SkipIfEnvironmentMissingTheory]
    [InlineData(SortOrder.unordered)]
    [InlineData(SortOrder.ascending)]
    [InlineData(SortOrder.descending)]
    public async Task GetAllAsync_ShouldReturnSortedList_WhenItemsAreFound(SortOrder sortOrder)
    {
        // Arrange
        List<Account> accounts = Enumerable.Range(5, 10).Select(_ => Fakes.GenerateAccount()).ToList();
        foreach (Account account in accounts)
        {
            await _sut.CreateAsync(account);
        }

        GetAllAccountsOptions options = new()
                                        {
                                            SortField = "username",
                                            SortOrder = sortOrder,
                                            Page = 1,
                                            PageSize = 25
                                        };

        // Act
        IEnumerable<Account> dbResult = await _sut.GetAllAsync(options);
        List<Account> results = dbResult.ToList();

        //Assert
        results.Should().NotBeEmpty();

        switch (sortOrder)
        {
            case SortOrder.ascending:
                results.Should().BeInAscendingOrder(x => x.Username, StringComparer.CurrentCulture);
                break;
            case SortOrder.descending:
                results.Should().BeInDescendingOrder(x => x.Username, StringComparer.CurrentCulture);
                break;
            case SortOrder.unordered:
            default:
                break;
        }
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetCountAsync_ShouldReturnZero_WhenItemsAreNotFound()
    {
        // Arrange
        List<Account> accounts = Enumerable.Range(5, 10).Select(x => Fakes.GenerateAccount()).ToList();
        foreach (Account account in accounts)
        {
            await _sut.CreateAsync(account);
        }

        // Act
        int result = await _sut.GetCountAsync("Bingus");

        // Assert
        result.Should().Be(0);
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetCountAsync_ShouldReturnCount_WhenItemsAreFound()
    {
        // Arrange
        List<Account> accounts = Enumerable.Range(5, 10).Select(x => Fakes.GenerateAccount()).ToList();
        foreach (Account account in accounts)
        {
            await _sut.CreateAsync(account);
        }

        Random random = new();

        Account accountToFind = accounts[random.Next(accounts.Count - 1)];

        // Act
        int result = await _sut.GetCountAsync(accountToFind.Username);

        // Assert
        result.Should().Be(1);
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailIsNotFound()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();

        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByEmailAsync("Bingus");

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingTheory]
    [MemberData(nameof(GetSingleSearchEmailData))]
    public async Task GetByEmailAsync_ShouldReturnAccount_WhenEmailIsFound(Account account, string email)
    {
        // Arrange
        account.ActivationCode = "Test";
        account.ActivationExpiration = DateTime.UtcNow;

        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenUsernameIsNotFound()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();

        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByUsernameAsync("Bingus");

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingTheory]
    [MemberData(nameof(GetSingleSearchUsernameData))]
    public async Task GetByUsernameAsync_ShouldReturnAccount_WhenUsernameIsFound(Account account, string username)
    {
        // Arrange
        account.ActivationCode = "Test";
        account.ActivationExpiration = DateTime.UtcNow;

        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnvironmentMissingFact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenAccountIsNotUpdated()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();

        // Act
        bool result = await _sut.UpdateAsync(account, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenAccountIsUpdated()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        Account account = Fakes.GenerateAccount(AccountStatus.active, AccountRole.standard);

        account.ActivationCode = "Test";
        account.ActivationExpiration = DateTime.UtcNow;

        await _sut.CreateAsync(account);

        _dateTimeProvider.GetUtcNow().Returns(now);

        AccountUpdateRequest request = new()
                                       {
                                           FirstName = "Updated First Name",
                                           LastName = "Updated Last Name",
                                           Username = "",
                                           Password = "",
                                           Email = "",
                                           AccountStatus = ctr.AccountStatus.banned,
                                           AccountRole = ctr.AccountRole.trusted
                                       };

        Account updatedAccount = request.ToAccount(account.Id);

        // Act
        bool result = await _sut.UpdateAsync(updatedAccount, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        Account? updatedRecord = await _sut.GetByIdAsync(account.Id);

        updatedRecord.Should().NotBeNull();
        updatedRecord.FirstName.Should().Be(request.FirstName);
        updatedRecord.LastName.Should().Be(request.LastName);
        updatedRecord.AccountStatus.Should().Be((AccountStatus)request.AccountStatus);
        updatedRecord.AccountRole.Should().Be((AccountRole)request.AccountRole);
        updatedRecord.UpdatedUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));

        updatedRecord.Should().BeEquivalentTo(account, options =>
                                                       {
                                                           options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>();
                                                           options.Excluding(x => x.FirstName);
                                                           options.Excluding(x => x.LastName);
                                                           options.Excluding(x => x.AccountRole);
                                                           options.Excluding(x => x.AccountStatus);
                                                           options.Excluding(x => x.UpdatedUtc);

                                                           return options;
                                                       });
    }

    [SkipIfEnvironmentMissingFact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenAccountIsNotDeleted()
    {
        // Arrange

        // Act
        bool result = await _sut.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenAccountIsDeleted()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        Account account = Fakes.GenerateAccount().WithActivation();

        await _sut.CreateAsync(account);
        await _sut.ActivateAsync(account);
        _dateTimeProvider.GetUtcNow().Returns(now);

        // Act
        bool result = await _sut.DeleteAsync(account.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        Account? deletedRecord = await _sut.GetByIdAsync(account.Id);
        deletedRecord.Should().NotBeNull();
        deletedRecord.DeletedUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByIdAsync_ShouldReturnFalse_WhenIdIsNotFound()
    {
        // Arrange

        // Act
        bool result = await _sut.ExistsByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByIdAsync_ShouldReturnTrue_WhenIdIsFound()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();

        await _sut.CreateAsync(account);

        // Act
        bool result = await _sut.ExistsByIdAsync(account.Id);

        // Assert
        result.Should().BeTrue();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByUsernameAsync_ShouldReturnFalse_WhenUsernameIsNotFound()
    {
        // Arrange

        // Act
        bool result = await _sut.ExistsByUsernameAsync("Test");

        // Assert
        result.Should().BeFalse();
    }

    [SkipIfEnvironmentMissingTheory]
    [MemberData(nameof(GetSingleSearchUsernameData))]
    public async Task ExistsByUsernameAsync_ShouldReturnTrue_WhenUsernameIsFound(Account account, string username)
    {
        // Arrange
        await _sut.CreateAsync(account);

        // Act
        bool result = await _sut.ExistsByUsernameAsync(username);

        // Assert
        result.Should().BeTrue();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenEmailIsNotFound()
    {
        // Arrange

        // Act
        bool result = await _sut.ExistsByEmailAsync("test@test.com");

        // Assert
        result.Should().BeFalse();
    }

    [SkipIfEnvironmentMissingTheory]
    [MemberData(nameof(GetSingleSearchEmailData))]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailIsFound(Account account, string email)
    {
        // Arrange
        await _sut.CreateAsync(account);

        // Act
        bool result = await _sut.ExistsByEmailAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ActivateAsync_ShouldReturnTrueAndNullActivationFields_WhenAccountIsActivated()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        account.ActivationCode = "Test";
        account.ActivationExpiration = DateTime.Now;

        await _sut.CreateAsync(account);

        // Act
        bool result = await _sut.ActivateAsync(account);

        // Assert
        result.Should().BeTrue();

        Account? activatedAccount = await _sut.GetByIdAsync(account.Id);
        activatedAccount.Should().NotBeNull();
        activatedAccount.ActivationCode.Should().BeNull();
        activatedAccount.ActivationExpiration.Should().BeNull();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task UpdateActivationAsync_ShouldReturnTrueAndUpdateActivation_WhenActivationInformationIsReRequested()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        Account account = Fakes.GenerateAccount();
        account.ActivationCode = "Old and Busted";
        account.ActivationExpiration = DateTime.MinValue;

        AccountActivation updatedAccountActivation = new()
                                                     {
                                                         Username = account.Username,
                                                         ActivationCode = "Test",
                                                         Expiration = now
                                                     };

        await _sut.CreateAsync(account);

        account.ActivationCode = updatedAccountActivation.ActivationCode;
        account.ActivationExpiration = updatedAccountActivation.Expiration;

        // Act
        bool result = await _sut.UpdateActivationAsync(account);

        // Assert
        result.Should().BeTrue();

        Account? updatedAccount = await _sut.GetByIdAsync(account.Id);
        updatedAccount.Should().NotBeNull();
        updatedAccount.ActivationCode.Should().Be(updatedAccountActivation.ActivationCode);
        updatedAccount.ActivationExpiration.Should().BeCloseTo(updatedAccountActivation.Expiration, TimeSpan.FromSeconds(1));
    }

    [SkipIfEnvironmentMissingTheory]
    [MemberData(nameof(GetSingleSearchEmailData))]
    public async Task RequestPasswordResetAsync_ShouldReturnTrueAndUpdatePasswordResetInformation_WhenPasswordIsRequestedToBeReset(Account account, string email)
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        _dateTimeProvider.GetUtcNow().Returns(now);

        const string resetCode = "069420";

        await _sut.CreateAsync(account);

        // Act
        bool result = await _sut.RequestPasswordResetAsync(email, resetCode);

        // Assert
        result.Should().BeTrue();

        Account? updatedResult = await _sut.GetByIdAsync(account.Id);

        updatedResult.Should().BeEquivalentTo(account, options =>
                                                       {
                                                           options.Excluding(x => x.PasswordResetRequestedUtc);
                                                           options.Excluding(x => x.ResetCode);
                                                           options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>();

                                                           return options;
                                                       });

        updatedResult.ResetCode.Should().Be("069420");
        updatedResult.PasswordResetRequestedUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ResetPasswordAsync_ShouldResetPassword_WhenPasswordIsReset()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        
        PasswordReset passwordReset = new()
                                      {
                                          Email = account.Email,
                                          Password = "TestPassword",
                                          ResetCode = "069420"
                                      };

        DateTime now = DateTime.UtcNow;
        _dateTimeProvider.GetUtcNow().Returns(now);

        await _sut.CreateAsync(account);

        Account expectedResult = account.Clone();
        expectedResult.Password = "TestPassword";
        expectedResult.PasswordResetRequestedUtc = null;
        expectedResult.ResetCode = null;
        expectedResult.UpdatedUtc = now;

        // Act
        bool result = await _sut.ResetPasswordAsync(passwordReset);

        // Assert
        result.Should().BeTrue();

        Account? updatedResult = await _sut.GetByIdAsync(account.Id);

        updatedResult.Should().BeEquivalentTo(expectedResult, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    public static IEnumerable<object[]> GetSingleSearchUsernameData()
    {
        Account account = Fakes.GenerateAccount();

        yield return [account, account.Username];
        yield return [account, account.Username.ToLowerInvariant()];
        yield return [account, account.Username.ToUpperInvariant()];
        yield return [account, account.Username.RandomizeCasing()];
    }

    public static IEnumerable<object[]> GetSingleSearchEmailData()
    {
        Account account = Fakes.GenerateAccount();

        yield return [account, account.Email];
        yield return [account, account.Email.ToLowerInvariant()];
        yield return [account, account.Email.ToUpperInvariant()];
        yield return [account, account.Email.RandomizeCasing()];
    }
}