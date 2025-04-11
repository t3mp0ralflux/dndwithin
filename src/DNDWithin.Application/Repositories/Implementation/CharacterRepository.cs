using System.Data;
using System.Transactions;
using Dapper;
using DNDWithin.Application.Database;
using DNDWithin.Application.Models;
using DNDWithin.Application.Models.Characters;
using DNDWithin.Application.Services.Implementation;

namespace DNDWithin.Application.Repositories.Implementation;

public class CharacterRepository : ICharacterRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CharacterRepository(IDbConnectionFactory dbConnectionFactory, IDateTimeProvider dateTimeProvider)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> CreateAsync(Character character, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         insert into character(id, account_id, name, username, created_utc, updated_utc, deleted_utc)
                                                                         values(@Id, @AccountId, @Name, @Username, @CreatedUtc, @UpdatedUtc, null)
                                                                         """, new { 
                                                                                      character.Id, 
                                                                                      character.AccountId, 
                                                                                      character.Name,
                                                                                      character.Username,
                                                                                      CreatedUtc = _dateTimeProvider.GetUtcNow(), 
                                                                                      UpdatedUtc = _dateTimeProvider.GetUtcNow(),
                                                                                  }, cancellationToken: token));

        if (result > 0)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                                                                insert into characteristics(id, character_id, gender, age, hair, eyes, skin, height, weight, faith)
                                                                values (@Id, @CharacterId, @Gender, @Age, @Hair, @Eyes, @Skin, @Height, @Weight, @Faith)
                                                                """, new
                                                                     {
                                                                         Id = Guid.NewGuid(),
                                                                         CharacterId = character.Id,
                                                                         character.Characteristics.Gender,
                                                                         character.Characteristics.Age,
                                                                         character.Characteristics.Hair,
                                                                         character.Characteristics.Eyes,
                                                                         character.Characteristics.Skin,
                                                                         character.Characteristics.Height,
                                                                         character.Characteristics.Weight,
                                                                         character.Characteristics.Faith
                                                                     }, cancellationToken: token));
        }
        
        transaction.Commit();

        return result > 0;
    }

    public async Task<Character?> GetByIdAsync(Guid id, bool includeDeleted = false, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var shouldIncludeDeleted = includeDeleted ? string.Empty : "and deleted_utc is null";

        IEnumerable<Character> result = await connection.QueryAsync<Character, Characteristics, Character>(new CommandDefinition($"""
                                                                                                                                 select c.id, c.account_id as AccountId, c.username, c.name, c.created_utc as CreatedUtc, c.updated_utc as UpdatedUtc, c.deleted_utc as DeletedUtc, 
                                                                                                                                 ch.gender, ch.age, ch.hair, ch.eyes, ch.skin, ch.height, ch.weight, ch.faith
                                                                                                                                 from character c left join characteristics ch on c.id = ch.character_id
                                                                                                                                 where c.id = @id
                                                                                                                                 {shouldIncludeDeleted}
                                                                                                                                 """, new { id }, cancellationToken: token), (character, characteristics) =>
                                                                                                                                                                          {
                                                                                                                                                                              character.Characteristics = characteristics;

                                                                                                                                                                              return character;
                                                                                                                                                                          }, splitOn: "gender");
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<Character>> GetAllAsync(GetAllCharactersOptions options, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        string orderClause = string.Empty;
        
        if (options.SortField is not null)
        {
            orderClause = $"order by {options.SortField} {(options.SortOrder == SortOrder.ascending ? "asc" : "desc")}";
        }

        IEnumerable<Character> results = await connection.QueryAsync<Character, Characteristics, Character>(new CommandDefinition($"""
                                                                                                                                   select c.id, c.account_id as AccountId, c.username, c.name, c.created_utc as CreatedUtc, c.updated_utc as UpdatedUtc, c.deleted_utc as DeletedUtc, 
                                                                                                                                  ch.Gender, ch.gender, ch.age, ch.hair, ch.eyes, ch.skin, ch.height, ch.weight, ch.faith
                                                                                                                                  from character c left join characteristics ch on c.id = ch.character_id
                                                                                                                                  where c.account_id = @AccountId
                                                                                                                                  and (@Name is null or lower(c.name) like ('%' || @Name || '%'))
                                                                                                                                  and c.deleted_utc is null
                                                                                                                                  {orderClause}
                                                                                                                                  """, new
                                                                                                                                       {
                                                                                                                                           options.AccountId,
                                                                                                                                           options.Name,
                                                                                                                                       }, cancellationToken: token), (character, characteristics) =>
                                                                                                                                                                     {
                                                                                                                                                                         character.Characteristics = characteristics;

                                                                                                                                                                         return character;
                                                                                                                                                                     }, splitOn: "gender");
        return results;
    }

    public async Task<int> GetCountAsync(GetAllCharactersOptions options, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        int result = await connection.QuerySingleAsync<int>(new CommandDefinition("""
                                                                                  select count(c.id)
                                                                                  from character c left join  characteristics ch on c.id = ch.character_id
                                                                                  where c.account_id = @AccountId
                                                                                  and (@Name is null or lower(c.name) like ('%' || @Name || '%'))
                                                                                  and c.deleted_utc is null
                                                                                  """, new
                                                                                       {
                                                                                           options.AccountId,
                                                                                           Name = options.Name?.ToLowerInvariant()
                                                                                       }, cancellationToken: token));

        return result;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var result = await connection.QuerySingleAsync<int>(new CommandDefinition("""
                                                                                  select count(id)
                                                                                  from character
                                                                                  where id = @id
                                                                                  """, new { id }, cancellationToken: token));

        return result > 0;
    }

    public async Task<bool> UpdateAsync(Character character, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        // note: don't update the dead
        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update character
                                                                         set name = @Name, updated_utc = @UpdatedUtc
                                                                         where id = @Id
                                                                         and deleted_utc is null
                                                                         """, new
                                                                              {
                                                                                  character.Name,
                                                                                  character.Id,
                                                                                  character.UpdatedUtc
                                                                              }, cancellationToken: token));

        if (result > 0)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                                                                update characteristics
                                                                set gender = @Gender, age = @Age, hair = @Hair, eyes = @Eyes, skin = @Skin, height = @Height, weight = @Weight, faith = @Faith
                                                                where character_id = @Id
                                                                """, new
                                                                     {
                                                                         character.Characteristics.Gender,
                                                                         character.Characteristics.Age,
                                                                         character.Characteristics.Hair,
                                                                         character.Characteristics.Eyes,
                                                                         character.Characteristics.Skin,
                                                                         character.Characteristics.Height,
                                                                         character.Characteristics.Weight,
                                                                         character.Characteristics.Faith,
                                                                         character.Id
                                                                     }, cancellationToken: token));
        }
        
        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         update character
                                                                         set deleted_utc = @DeletedUtc
                                                                         where id = @id
                                                                         """, new
                                                                              {
                                                                                  DeletedUtc = _dateTimeProvider.GetUtcNow(),
                                                                                  id
                                                                              }, cancellationToken: token));
        transaction.Commit();

        return result > 0;
    }
}