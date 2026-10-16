using System.Data;
using Activout.DatabaseClient.Dapper.Extensions.DependencyInjection;
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

        services.AddDapperDatabaseClient<IUserDatabase>();
        services.AddDapperDatabaseClient<IPublisherDatabase>();
        services.AddDapperDatabaseClient<IReportDatabase>();
        services.AddDapperDatabaseClient<IShoppingCartDatabase>();
        services.AddDapperDatabaseClient<ICreditCardDatabase>();
        services.AddDapperDatabaseClient<ICustomerOrderDatabase>();
        services.AddDapperDatabaseClient<IAdminOrderDatabase>();
        services.AddDapperDatabaseClient<IBookDatabase>();

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
}
