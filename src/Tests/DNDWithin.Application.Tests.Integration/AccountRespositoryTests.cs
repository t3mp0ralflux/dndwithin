using DNDWithin.Api.Mapping;
using DNDWithin.Application.Database;
using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;
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
    public readonly IDateTimeProvider DateTimeProvider = Substitute.For<IDateTimeProvider>();

    public AccountRespositoryTests(ApplicationApiFactory apiFactory)
    {
        IDbConnectionFactory connectionFactory = apiFactory.Services.GetRequiredService<IDbConnectionFactory>();

        _sut = new AccountRepository(connectionFactory, DateTimeProvider);
    }

    public AccountRepository _sut { get; set; }


    [SkipIfEnvironmentMissingFact]
    public async Task CreateAsync_ShouldCreateAccount_WhenDataIsPassed()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };

        // Act
        bool result = await _sut.CreateAsync(account, activation);

        // Assert
        result.Should().BeTrue();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task UsernameExistsAsync_ShouldReturnNull_WhenUsernameDoesNotExist()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };
        
        await _sut.CreateAsync(account, activation);

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
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };
        
        await _sut.CreateAsync(account, activation);

        // Act
        Account? result = await _sut.GetByUsernameAsync(account.Username);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdIsNotFound()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };
        
        await _sut.CreateAsync(account, activation);

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
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };
        
        await _sut.CreateAsync(account, activation);

        // Act
        Account? result = await _sut.GetByIdAsync(account.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnivronmentMissingTheory]
    [InlineData(AccountStatus.banned, null, null)]
    [InlineData(null, AccountRole.standard, null)]
    [InlineData(null, null, "Bingus")]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNothingIsFound(AccountStatus? accountStatus, AccountRole? accountRole, string? userName)
    {
        // Arrange
        Account account = Fakes.GenerateAccount(); // defaults to Active, Admin
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };
        
        GetAllAccountsOptions options = new()
                                        {
                                            AccountStatus = accountStatus,
                                            AccountRole = accountRole,
                                            UserName = userName,
                                            Page = 1,
                                            PageSize = 10
                                        };
        await _sut.CreateAsync(account, activation);

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
            var activation = new AccountActivation()
                             {
                                 Username = account.Username,
                                 ActivationCode = "Test",
                                 Expiration = DateTime.UtcNow
                             };
            
            await _sut.CreateAsync(account, activation);
        }

        IEnumerable<Account> expectedResult = [accountToFind];

        // Act
        IEnumerable<Account> enumerableResult = await _sut.GetAllAsync(getAllOptions);
        List<Account> result = enumerableResult.ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(expectedResult, options => options
                                                                  .Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1)))
                                                                  .WhenTypeIs<DateTime>());
    }

    [Theory]
    [InlineData(SortOrder.unordered)]
    [InlineData(SortOrder.ascending)]
    [InlineData(SortOrder.descending)]
    public async Task GetAllAsync_ShouldReturnSortedList_WhenItemsAreFound(SortOrder sortOrder)
    {
         // Arrange
         var accounts = Enumerable.Range(5, 10).Select(x => Fakes.GenerateAccount()).ToList();
         foreach (Account account in accounts)
         {
             var activation = new AccountActivation()
                              {
                                  Username = account.Username,
                                  ActivationCode = "Test",
                                  Expiration = DateTime.UtcNow
                              };
             
             await _sut.CreateAsync(account, activation);
         }

         var options = new GetAllAccountsOptions()
                       {
                           SortField = "username",
                           SortOrder = sortOrder,
                           Page = 1,
                           PageSize = 25,
                       };

         IEnumerable<Account> expectedAccountOrder = sortOrder switch
         {
             SortOrder.ascending => accounts.OrderBy(x => x.Username),
             SortOrder.descending => accounts.OrderByDescending(x => x.Username),
             _ => accounts
         };

         // Act
         var dbResult = await _sut.GetAllAsync(options);
         var results = dbResult.ToList();

         results.Should().NotBeEmpty();

         switch (sortOrder)
         {
             // Assert
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
            var activation = new AccountActivation()
                             {
                                 Username = account.Username,
                                 ActivationCode = "Test",
                                 Expiration = DateTime.UtcNow
                             };
            
            await _sut.CreateAsync(account, activation);
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
            var activation = new AccountActivation()
                             {
                                 Username = account.Username,
                                 ActivationCode = "Test",
                                 Expiration = DateTime.UtcNow
                             };
            
            await _sut.CreateAsync(account, activation);
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
        Account? account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };
        

        await _sut.CreateAsync(account, activation);
        
        // Act
        Account? result = await _sut.GetByEmailAsync("Bingus");

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByEmailAsync_ShouldReturnAccount_WhenEmailIsFound()
    {
        // Arrange
        Account? account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };

        await _sut.CreateAsync(account, activation);
        
        // Act
        Account? result = await _sut.GetByEmailAsync(account.Email);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(account, options => options.Using<DateTime>(x => x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenUsernameIsNotFound()
    {
        // Arrange
        Account? account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };

        await _sut.CreateAsync(account, activation);
        
        // Act
        Account? result = await _sut.GetByUsernameAsync("Bingus");

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByUsernameAsync_ShouldReturnAccount_WhenUsernameIsFound()
    {
        // Arrange
        Account? account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };

        await _sut.CreateAsync(account, activation);
        
        // Act
        Account? result = await _sut.GetByUsernameAsync(account.Username);

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
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };

        await _sut.CreateAsync(account, activation, CancellationToken.None);

        DateTimeProvider.GetUtcNow().Returns(now);

        AccountUpdateRequest request = new()
                                       {
                                           FirstName = "Updated First Name",
                                           LastName = "Updated Last Name",
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

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenAccountIsDeleted()
    {
        // Arrange
        DateTime now = DateTime.UtcNow;
        Account account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };

        await _sut.CreateAsync(account, activation, CancellationToken.None);
        DateTimeProvider.GetUtcNow().Returns(now);

        // Act
        bool result = await _sut.DeleteAsync(account.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        Account? deletedRecord = await _sut.GetByIdAsync(account.Id);
        deletedRecord.Should().NotBeNull();
        deletedRecord.DeletedUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByIdAsync_ShouldReturnNull_WhenIdIsNotFound()
    {
        // Arrange

        // Act
        var result = await _sut.ExistsByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByIdAsync_ShouldReturnAccount_WhenIdIsFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };
        
        await _sut.CreateAsync(account, activation);
        
        // Act
        var result = await _sut.ExistsByIdAsync(account.Id);

        // Assert
        result.Should().NotBeNull();
    }
    
    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByUsernameAsync_ShouldReturnNull_WhenUsernameIsNotFound()
    {
        // Arrange

        // Act
        var result = await _sut.ExistsByUsernameAsync("Test");

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByUsernameAsync_ShouldReturnAccount_WhenUsernameIsFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };
        
        await _sut.CreateAsync(account, activation);
        
        // Act
        var result = await _sut.ExistsByUsernameAsync(account.Username);

        // Assert
        result.Should().NotBeNull();
    }
    
    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByEmailAsync_ShouldReturnNull_WhenEmailIsNotFound()
    {
        // Arrange

        // Act
        var result = await _sut.ExistsByEmailAsync("test@test.com");

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByEmailAsync_ShouldReturnAccount_WhenEmailIsFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var activation = new AccountActivation()
                         {
                             Username = account.Username,
                             ActivationCode = "Test",
                             Expiration = DateTime.UtcNow
                         };
        
        await _sut.CreateAsync(account, activation);
        
        // Act
        var result = await _sut.ExistsByEmailAsync(account.Email!);

        // Assert
        result.Should().NotBeNull();
    }
}