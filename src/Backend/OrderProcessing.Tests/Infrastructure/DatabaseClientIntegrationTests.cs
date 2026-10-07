using Activout.DatabaseClient;
using Activout.DatabaseClient.Attributes;
using Activout.DatabaseClient.Dapper;
using Activout.DatabaseClient.Implementation;
using Npgsql;
using Xunit;

namespace OrderProcessing.Tests.Infrastructure;

public class DatabaseClientIntegrationTests
{
    [SkippableFact]
    [Trait("Category", "Integration")]
    public async Task DatabaseClient_ExecutesQueriesAndTransactionOnPostgres()
    {
        var connectionString = Environment.GetEnvironmentVariable("ORDER_PROCESSING_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Skip.If(true, "Set ORDER_PROCESSING_TEST_CONNECTION_STRING to run PostgreSQL integration tests.");
        }

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
