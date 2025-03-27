using DNDWithin.Application.Database;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Repositories.Implementation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Testing.Common;

namespace DNDWithin.Application.Tests.Integration;

public class AccountRespositoryTests : IClassFixture<ApplicationApiFactory>
{
    public AccountRespositoryTests(ApplicationApiFactory apiFactory)
    {
        _sut = new AccountRepository(apiFactory.Services.GetRequiredService<IDbConnectionFactory>());
    }

    public AccountRepository _sut { get; set; }


    [SkipIfEnvironmentMissingFact]
    public async Task CreateAsync_ShouldCreateAccount_WhenDataIsPassed()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();

        // Act
        bool result = await _sut.CreateAsync(account);

        // Assert
        result.Should().BeTrue();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task UsernameExistsAsync_ShouldReturnNull_WhenUsernameDoesNotExist()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByUsernameAsync("BigChungus");

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task UsernameExistsAsync_ShouldReturnAccount_WhenUsernameExists()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByUsernameAsync(account.Username);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromMinutes(1))).WhenTypeIs<DateTime>());
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
        await _sut.CreateAsync(account);

        // Act
        Account? result = await _sut.GetByIdAsync(account.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromMinutes(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnivronmentMissingTheory]
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
                                            UserName = userName
                                        };
        await _sut.CreateAsync(account);

        // Act
        IEnumerable<Account> result = await _sut.GetAllAsync(options);

        // Assert
        result.Should().BeEmpty();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetAllAsync_ShouldReturnListWithOneAccount_WhenItemIsFound()
    {
        // Arrange

        // defaults to Active, Admin
        List<Account> accounts = Enumerable.Range(5, 10).Select(x => Fakes.GenerateAccount()).ToList();

        Random random = new();

        Account accountToFind = accounts[random.Next(accounts.Count - 1)];

        GetAllAccountsOptions getAllOptions = new()
                                              {
                                                  AccountStatus = accountToFind.AccountStatus,
                                                  AccountRole = accountToFind.AccountRole,
                                                  UserName = accountToFind.Username,
                                                  Page = 1,
                                                  PageSize = 5
                                              };
        foreach (Account account in accounts)
        {
            await _sut.CreateAsync(account);
        }

        IEnumerable<Account> expectedResult = [accountToFind];

        // Act
        IEnumerable<Account> enumerableResult = await _sut.GetAllAsync(getAllOptions);
        List<Account> result = enumerableResult.ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(expectedResult, options => options
                                                                  .Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromMinutes(1)))
                                                                  .WhenTypeIs<DateTime>());
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
        var result = await _sut.GetCountAsync("Bingus");

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

        var random = new Random();

        var accountToFind = accounts[random.Next(accounts.Count - 1)];

        // Act
        var result = await _sut.GetCountAsync(accountToFind.Username);

        // Assert
        result.Should().Be(1);
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailIsNotFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();

        await _sut.CreateAsync(account);
        // Act
        var result = await _sut.GetByEmailAsync("Bingus");

        // Assert
        result.Should().BeNull();
    }
    
    [SkipIfEnvironmentMissingFact]
    public async Task GetByEmailAsync_ShouldReturnAccount_WhenEmailIsFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();

        await _sut.CreateAsync(account);
        // Act
        var result = await _sut.GetByEmailAsync(account.Email);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x=>x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromMinutes(1))).WhenTypeIs<DateTime>());
    }
    
    [SkipIfEnvironmentMissingFact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenUsernameIsNotFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();

        await _sut.CreateAsync(account);
        // Act
        var result = await _sut.GetByUsernameAsync("Bingus");

        // Assert
        result.Should().BeNull();
    }
    
    [SkipIfEnvironmentMissingFact]
    public async Task GetByUsernameAsync_ShouldReturnAccount_WhenUsernameIsFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();

        await _sut.CreateAsync(account);
        // Act
        var result = await _sut.GetByUsernameAsync(account.Username);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x=>x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromMinutes(1))).WhenTypeIs<DateTime>());
    }
    
    
}