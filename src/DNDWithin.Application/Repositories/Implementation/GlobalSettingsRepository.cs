using System.Data;
using Dapper;
using DNDWithin.Application.Database;
using DNDWithin.Application.Models;
using DNDWithin.Application.Models.GlobalSettings;

namespace DNDWithin.Application.Repositories.Implementation;

public class GlobalSettingsRepository : IGlobalSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GlobalSettingsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }


    public async Task<bool> CreateSetting(GlobalSetting setting, CancellationToken token = default)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(token);
        int result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         insert into globalsettings(id, name, value)
                                                                         values(@id, @name, @value)
                                                                         """, setting, cancellationToken: token));

        return result > 0;
    }

    public async Task<GlobalSetting?> GetSetting(string name, CancellationToken token = default)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<GlobalSetting>(new CommandDefinition("""
                                                                                               select * from globalsettings
                                                                                               where name = @name
                                                                                               """, new { name }, cancellationToken: token));
    }

    public async Task<IEnumerable<GlobalSetting>> GetAllAsync(GetAllGlobalSettingsOptions options, CancellationToken token = default)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(token);

        string orderClause = string.Empty;

        if (options.SortField is not null)
        {
            orderClause = $"order by {options.SortField} {(options.SortOrder == SortOrder.ascending ? "asc" : "desc")}";
        }

        IEnumerable<GlobalSetting> results = await connection.QueryAsync<GlobalSetting>(new CommandDefinition($"""
                                                                                                               select * from globalsettings
                                                                                                               where (@name is null or name like ('%' || @name || '%'))
                                                                                                               {orderClause}
                                                                                                               limit @pageSize
                                                                                                               offset @pageOffset
                                                                                                               """, new
                                                                                                                    {
                                                                                                                        name = options.Name,
                                                                                                                        pageSize = options.PageSize,
                                                                                                                        pageOffset = (options.Page - 1) * options.PageSize
                                                                                                                    }, cancellationToken: token));

        return results;
    }

    public async Task<int> GetCountAsync(string name, CancellationToken token = default)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleAsync<int>(new CommandDefinition("""
                                                                            select count(id)
                                                                            from globalsettings
                                                                            where (@name is null or name like ('%' || @name || '%'))
                                                                            """, new { name }, cancellationToken: token));
    }
}