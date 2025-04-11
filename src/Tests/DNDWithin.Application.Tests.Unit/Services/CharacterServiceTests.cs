using DNDWithin.Application.Models.Characters;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Services.Implementation;
using DNDWithin.Application.Validators.Characters;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Testing.Common;

namespace DNDWithin.Application.Tests.Unit.Services;

public class CharacterServiceTests
{
    public CharacterServiceTests()
    {
        _sut = new CharacterService(_characterRepository, _characterValidator, _optionsValidator);
    }

    public CharacterService _sut { get; set; }
    private readonly ICharacterRepository _characterRepository = Substitute.For<ICharacterRepository>();
    private readonly IValidator<Character> _characterValidator = new CharacterValidator();
    private readonly IValidator<GetAllCharactersOptions> _optionsValidator = new GetAllCharactersOptionsValidator();

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenValidatorFails()
    {
        // Arrange
        var account = Fakes.GenerateAccount();

        var character = Fakes.GenerateCharacter(account);
        character.Name = string.Empty;
        
        // Act
        var action = async () => await _sut.CreateAsync(character);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnFalse_WhenCharacterIsNotCreated()
    {
        // Arrange
        var account = Fakes.GenerateAccount();

        var character = Fakes.GenerateCharacter(account);

        _characterRepository.CreateAsync(Arg.Any<Character>()).Returns(false);

        // Act
        var result = await _sut.CreateAsync(character);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task CreateAsync_ShouldReturnTrue_WhenCharacterIsCreated()
    {
        // Arrange
        var account = Fakes.GenerateAccount();

        var character = Fakes.GenerateCharacter(account);

        _characterRepository.CreateAsync(Arg.Any<Character>()).Returns(true);

        // Act
        var result = await _sut.CreateAsync(character);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenCharacterIsNotFound()
    {
        // Arrange
        _characterRepository.GetByIdAsync(Guid.NewGuid()).Returns((Character?)null);
        
        // Act
        var result = await _sut.GetAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCharacter_WhenCharacterIsFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var character = Fakes.GenerateCharacter(account);

        _characterRepository.GetByIdAsync(character.Id).Returns(character);

        // Act
        var result = await _sut.GetAsync(character.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(character);
    }

    [Fact]
    public async Task GetAllAsync_ShouldThrowValidationException_WhenValidatorFails()
    {
        // Arrange
        var options = new GetAllCharactersOptions()
                      {
                          AccountId = Guid.NewGuid(),
                          Page = 1,
                          PageSize = 5,
                          SortField = "stat"
                      };
        // Act
        var action = async () => await _sut.GetAllAsync(options);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoItemsAreFound()
    {
        // Arrange
        var options = new GetAllCharactersOptions()
                      {
                          AccountId = Guid.NewGuid(),
                          Page = 1,
                          PageSize = 5
                      };

        _characterRepository.GetAllAsync(Arg.Any<GetAllCharactersOptions>()).Returns([]);
        
        // Act
        var result = await _sut.GetAllAsync(options);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnItems_WhenItemsAreFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        
        var options = new GetAllCharactersOptions()
                      {
                          AccountId = account.Id,
                          Page = 1,
                          PageSize = 5
                      };

        var characters = Enumerable.Range(1, 5).Select(x => Fakes.GenerateCharacter(account)).ToList();

        _characterRepository.GetAllAsync(Arg.Any<GetAllCharactersOptions>()).Returns(characters);

        // Act
        var result = await _sut.GetAllAsync(options);

        // Assert
        var resultList = result.ToList();
        
        resultList.Should().NotBeNullOrEmpty();
        resultList.Should().BeEquivalentTo(characters);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task GetCountAsync_ShouldReturnCorrectCount_WhenItemsAreFoundOrNot(int count)
    {
        // Arrange
        var options = new GetAllCharactersOptions()
                     {
                         AccountId = Guid.NewGuid(),
                         Page = 1,
                         PageSize = 5
                     };
        
        _characterRepository.GetCountAsync(Arg.Any<GetAllCharactersOptions>()).Returns(count);
        
        // Act
        var result = await _sut.GetCountAsync(options);

        // Assert
        result.Should().Be(count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenAccountIsNotFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var character = Fakes.GenerateCharacter(account);

        _characterRepository.ExistsByIdAsync(character.Id).Returns(false);
        
        // Act
        var result = await _sut.UpdateAsync(character);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenDatabaseFailsToUpdate()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var character = Fakes.GenerateCharacter(account);
        
        _characterRepository.ExistsByIdAsync(character.Id).Returns(true);
        _characterRepository.UpdateAsync(Arg.Any<Character>()).Returns(false);
        
        // Act
        var result = await _sut.UpdateAsync(character);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldReturnTrue_WhenCharacterIsUpdated()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var character = Fakes.GenerateCharacter(account);
        
        _characterRepository.ExistsByIdAsync(character.Id).Returns(true);
        _characterRepository.UpdateAsync(Arg.Any<Character>()).Returns(true);
        
        // Act
        var result = await _sut.UpdateAsync(character);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCharacterIsNotFound()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var character = Fakes.GenerateCharacter(account);

        _characterRepository.ExistsByIdAsync(character.Id).Returns(false);
        
        // Act
        var result = await _sut.DeleteAsync(character.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenDatabaseFailsToDelete()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var character = Fakes.GenerateCharacter(account);

        _characterRepository.ExistsByIdAsync(character.Id).Returns(true);
        _characterRepository.DeleteAsync(character.Id).Returns(false);
        
        // Act
        var result = await _sut.DeleteAsync(character.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenDatabaseDeletesCharacter()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        var character = Fakes.GenerateCharacter(account);

        _characterRepository.ExistsByIdAsync(character.Id).Returns(true);
        _characterRepository.DeleteAsync(character.Id).Returns(true);
        
        // Act
        var result = await _sut.DeleteAsync(character.Id);

        // Assert
        result.Should().BeTrue();
    }
}