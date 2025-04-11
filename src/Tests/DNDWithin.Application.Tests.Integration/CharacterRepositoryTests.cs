using DNDWithin.Application.Database;
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

    [Fact]
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
}