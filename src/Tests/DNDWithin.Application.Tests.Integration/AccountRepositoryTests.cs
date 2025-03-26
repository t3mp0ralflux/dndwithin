using Dapper;
using DNDWithin.Application.Database;
using DNDWithin.Application.Repositories;
using DNDWithin.Application.Repositories.Implementation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Testing.Common;
using Xunit.Abstractions;

namespace DNDWithin.Application.Tests.Integration;

public class AccountRepositoryTests : IClassFixture<ApplicationTestFactory>
{
    private readonly IAccountRepository _sut;
    
    public AccountRepositoryTests(ApplicationTestFactory applicationTestFactory, ITestOutputHelper output)
    {
        IDbConnectionFactory dbConnectionFactory = applicationTestFactory.Services.GetRequiredService<IDbConnectionFactory>();
        _sut = new AccountRepository(dbConnectionFactory);

        SetupTest(dbConnectionFactory).Wait();
    }

    protected async Task SetupTest(IDbConnectionFactory dbConnectionFactory)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(new CommandDefinition("""
                                                            create table if not exists account(
                                                                id UUID primary key,
                                                                first_name varchar(25) not null,
                                                                last_name varchar(25) not null,
                                                                username varchar(25) not null,
                                                                password varchar not null,
                                                                email varchar(50) not null,
                                                                created_utc timestamp not null,
                                                                updated_utc timestamp not null,
                                                                last_login_utc timestamp not null,
                                                                deleted_utc timestamp null,
                                                                account_status int not null,
                                                                account_role int not null
                                                            ); 
                                                            """));
    }

    [Fact]
    public async Task CreateAsync_CreatesAccount_WhenInformationIsEntered()
    {
        // Arrange
        var account = Fakes.GenerateAccount();
        
        // Act
        var result = await _sut.CreateAsync(account, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }
    
}