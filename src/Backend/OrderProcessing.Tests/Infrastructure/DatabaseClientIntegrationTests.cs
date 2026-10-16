using Activout.DatabaseClient;
using Activout.DatabaseClient.Attributes;
using Activout.DatabaseClient.Dapper;
using Activout.DatabaseClient.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OrderProcessing.Infrastructure;
using OrderProcessing.Infrastructure.Data;
using Xunit;

namespace OrderProcessing.Tests.Infrastructure;

public class DatabaseClientIntegrationTests
{
    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task DatabaseClient_ExecutesQueriesAndTransactionOnPostgres()
    {
        var connectionString = GetConnectionString();

        await using var connection = new NpgsqlConnection(connectionString);
        var database = new DatabaseClientBuilder()
            .With(new DapperGateway(connection))
            .Build<IProbeDatabase>();

        await database.CreateTableAsync();
        using var transaction = database.BeginTransaction();
        await database.InsertAsync(42, transaction);
        transaction.Commit();

        Assert.Equal(42, await database.GetValueAsync());
    }

    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task InfrastructureRegistration_ResolvesCollectionDao()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(GetConnectionString());

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IAdminOrderDatabase>();

        var orders = await database.GetAllAsync();

        Assert.IsAssignableFrom<IReadOnlyList<OrderProcessing.Domain.Entities.AdminOrder>>(orders);
    }

    private static string GetConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("ORDER_PROCESSING_TEST_CONNECTION_STRING");
        Skip.If(string.IsNullOrWhiteSpace(connectionString),
            "Set ORDER_PROCESSING_TEST_CONNECTION_STRING to run PostgreSQL integration tests.");
        return connectionString!;
    }

    private interface IProbeDatabase : IWithTransactions
    {
        [SqlUpdate("CREATE TEMPORARY TABLE activout_probe (value integer NOT NULL)")]
        Task CreateTableAsync();

        [SqlUpdate("INSERT INTO activout_probe (value) VALUES (@value)")]
        Task InsertAsync(int value, System.Data.IDbTransaction transaction);

        [SqlQuery("SELECT value FROM activout_probe LIMIT 1")]
        Task<int> GetValueAsync();
    }
}
