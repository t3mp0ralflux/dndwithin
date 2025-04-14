using DNDWithin.Application.Database;
using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Accounts;
using DNDWithin.Application.Models.Characters;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Repositories.Implementation;
using DNDWithin.Application.Services.Implementation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Testing.Common;

namespace DNDWithin.Application.Tests.Integration;

public class CharacterRepositoryTests : IClassFixture<ApplicationApiFactory>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    public CharacterRepositoryTests(ApplicationApiFactory apiFactory)
    {
        IDbConnectionFactory connectionFactory = apiFactory.Services.GetRequiredService<IDbConnectionFactory>();

        _sut = new CharacterRepository(connectionFactory, _dateTimeProvider);
        _accountRepository = new AccountRepository(connectionFactory, _dateTimeProvider);
    }

    public CharacterRepository _sut { get; set; }

    [SkipIfEnvironmentMissingFact]
    public async Task CreateAsync_ShouldCreateCharacterWithoutCharacteristics_WhenNewCharacterIsCreated()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateNewCharacter(account);

        await _accountRepository.CreateAsync(account);

        // Act
        bool result = await _sut.CreateAsync(character);

        // Assert
        result.Should().BeTrue();

        Character? createdCharacter = await _sut.GetByIdAsync(character.Id);
        createdCharacter.Should().NotBeNull();
        createdCharacter.Should().BeEquivalentTo(character);
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCharacterIsNotFound()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);

        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

        // Act
        Character? result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetByIdAsync_ShouldReturnCharacter_WhenIdIsFound()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);

        var now = DateTime.UtcNow;
        _dateTimeProvider.GetUtcNow().Returns(now);

        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

        // Act
        Character? result = await _sut.GetByIdAsync(character.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(character, options => options.Using<DateTime>(x=>x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnvironmentMissingTheory]
    [InlineData(null, null, "Bingus")]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoItemsAreFound(string? characterClass, int? level, string? name)
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);

        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

        GetAllCharactersOptions options = new()
                                          {
                                              AccountId = account.Id,
                                              Page = 1,
                                              PageSize = 5,
                                              Class = characterClass,
                                              Level = level,
                                              Name = name
                                          };

        // Act
        IEnumerable<Character> result = await _sut.GetAllAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    
    [SkipIfEnvironmentMissingTheory]
    [MemberData(nameof(GetSingleSearchCharacterNameData))]
    public async Task GetAllAsync_ShouldReturnItems_WhenItemsAreFound(Account account, Character character, string characterName)
    {
        // Arrange
        await _accountRepository.CreateAsync(account);

        if (!await _sut.ExistsByIdAsync(character.Id))
        {
            await _sut.CreateAsync(character);
        }

        GetAllCharactersOptions options = new()
                                          {
                                              AccountId = account.Id,
                                              Page = 1,
                                              PageSize = 5,
                                              Name = characterName
                                          };

        // Act
        IEnumerable<Character> result = await _sut.GetAllAsync(options);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [SkipIfEnvironmentMissingTheory]
    [InlineData(SortOrder.unordered)]
    [InlineData(SortOrder.ascending)]
    [InlineData(SortOrder.descending)]
    public async Task GetAllAsync_ShouldReturnSortedList_WhenItemsAreSorted(SortOrder sortOrder)
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        List<Character> characters = Enumerable.Range(5, 10).Select(_ => Fakes.GenerateCharacter(account)).ToList();

        await _accountRepository.CreateAsync(account);
        foreach (Character character in characters)
        {
            await _sut.CreateAsync(character);
        }

        GetAllCharactersOptions options = new()
                                          {
                                              AccountId = account.Id,
                                              SortField = "name",
                                              SortOrder = sortOrder,
                                              Page = 1,
                                              PageSize = 25
                                          };

        // Act
        IEnumerable<Character> dbResult = await _sut.GetAllAsync(options);
        List<Character> results = dbResult.ToList();

        // Assert
        results.Should().NotBeEmpty();

        switch (sortOrder)
        {
            case SortOrder.ascending:
                results.Should().BeInAscendingOrder(x => x.Name, StringComparer.CurrentCulture);
                break;
            case SortOrder.descending:
                results.Should().BeInDescendingOrder(x => x.Name, StringComparer.CurrentCulture);
                break;
            case SortOrder.unordered:
            default:
                break;
        }
    }

    [Fact]
    public async Task GetCountAsync_ShouldReturnZero_WhenNoItemsAreCounted()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);

        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

        GetAllCharactersOptions options = new()
                                          {
                                              AccountId = account.Id,
                                              Page = 1,
                                              PageSize = 5,
                                              Name = "Bingus"
                                          };
        // Act
        int result = await _sut.GetCountAsync(options);

        // Assert
        result.Should().Be(0);
    }

    [SkipIfEnvironmentMissingFact]
    public async Task GetCountAsync_ShouldReturnOne_WhenItemsAreCounted()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);

        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

        GetAllCharactersOptions options = new()
                                          {
                                              AccountId = account.Id,
                                              Page = 1,
                                              PageSize = 5,
                                              Name = character.Name
                                          };
        // Act
        int result = await _sut.GetCountAsync(options);

        // Assert
        result.Should().Be(1);
    }

    [SkipIfEnvironmentMissingFact]
    public async Task ExistsByIdAsync_ShouldReturnFalse_WhenCharacterIsNotFound()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);

        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

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
        Character character = Fakes.GenerateCharacter(account);

        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

        // Act
        bool result = await _sut.ExistsByIdAsync(character.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCharacter_WhenUpdatesAreMade()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);

        DateTime now = DateTime.UtcNow;
        _dateTimeProvider.GetUtcNow().Returns(now);
        
        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

        Character update = new()
                           {
                               Id = character.Id,
                               AccountId = account.Id,
                               Name = "Bingus",
                               Username = account.Username,
                               CreatedUtc = now,
                               UpdatedUtc = now,
                               Characteristics = new Characteristics
                                                 {
                                                     Age = "420",
                                                     Eyes = "Present",
                                                     Faith = "Yes",
                                                     Gender = "All",
                                                     Hair = "Long and luscious",
                                                     Height = "Miniscule",
                                                     Skin = "Scaly",
                                                     Weight = "Heckin' Chonker"
                                                 }
                           };

        // Act
        bool result = await _sut.UpdateAsync(update);

        // Assert
        result.Should().BeTrue();

        Character? updatedCharacter = await _sut.GetByIdAsync(update.Id);
        updatedCharacter.Should().NotBeNull();
        updatedCharacter.Should().BeEquivalentTo(update, options => options.Using<DateTime>(x=>x.Subject.Should().BeCloseTo(x.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>());
    }

    [SkipIfEnvironmentMissingFact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCharacterIsNotDeleted()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);

        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

        // Act
        bool result = await _sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenCharacterIsDeleted()
    {
        // Arrange
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);

        await _accountRepository.CreateAsync(account);
        await _sut.CreateAsync(character);

        // Act
        bool result = await _sut.DeleteAsync(character.Id);

        // Assert
        result.Should().BeTrue();

        Character? deletedCharacter = await _sut.GetByIdAsync(character.Id, true);

        deletedCharacter.Should().NotBeNull();
        deletedCharacter.DeletedUtc.Should().NotBeNull();
    }
    
    public static IEnumerable<object[]> GetSingleSearchCharacterNameData()
    {
        Account account = Fakes.GenerateAccount();
        Character character = Fakes.GenerateCharacter(account);
        
        yield return [account, character, character.Name];
        yield return [account, character, character.Name.ToLowerInvariant()];
        yield return [account, character, character.Name.ToUpperInvariant()];
        yield return [account, character, character.Name.RandomizeCasing()];
    }
}