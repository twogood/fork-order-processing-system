using System.Data;
using Activout.DatabaseClient;
using Activout.DatabaseClient.Dapper;
using Activout.DatabaseClient.Implementation;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessing.Domain.Interfaces.Repositories;
using OrderProcessing.Infrastructure.Data;
using OrderProcessing.Infrastructure.Repositories;
using Npgsql;

namespace OrderProcessing.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
        services.AddTransient<DatabaseClientBuilder>();

        services.AddScoped<IUserDatabase>(CreateDatabaseClient<IUserDatabase>);
        services.AddScoped<IPublisherDatabase>(CreateDatabaseClient<IPublisherDatabase>);
        services.AddScoped<IReportDatabase>(CreateDatabaseClient<IReportDatabase>);
        services.AddScoped<IShoppingCartDatabase>(CreateDatabaseClient<IShoppingCartDatabase>);
        services.AddScoped<ICreditCardDatabase>(CreateDatabaseClient<ICreditCardDatabase>);
        services.AddScoped<ICustomerOrderDatabase>(CreateDatabaseClient<ICustomerOrderDatabase>);
        services.AddScoped<IAdminOrderDatabase>(CreateDatabaseClient<IAdminOrderDatabase>);
        services.AddScoped<IBookDatabase>(CreateDatabaseClient<IBookDatabase>);

        // Register repositories
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminOrderRepository, AdminOrderRepository>();
        services.AddScoped<ICustomerOrderRepository, CustomerOrderRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<ICreditCardRepository, CreditCardRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IPublisherRepository, PublisherRepository>();


        return services;
    }

    private static T CreateDatabaseClient<T>(IServiceProvider serviceProvider)
        where T : class =>
        serviceProvider.GetRequiredService<DatabaseClientBuilder>()
            .With(new DapperGateway(serviceProvider.GetRequiredService<IDbConnection>()))
            .Build<T>();
}
