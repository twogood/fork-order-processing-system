using System.Data;
using Activout.DatabaseClient;
using Activout.DatabaseClient.Attributes;
using OrderProcessing.Domain.Entities;

namespace OrderProcessing.Infrastructure.Data;

public interface ICustomerOrderDatabase : IWithTransactions
{
    [SqlQuery("""
        SELECT OrderID AS OrderNumber, OrderDate, "Status", TotalPrice, Custname AS Username, ShippingAddress
        FROM customerorder WHERE CustName = @username ORDER BY OrderDate DESC, OrderID DESC
        """)]
    Task<IReadOnlyList<CustomerOrder>> GetByUsernameAsync(string username);

    [SqlQuery("""
        SELECT OrderID AS OrderNumber, TotalPrice, "Status", OrderDate, CustName AS Username, ShippingAddress
        FROM customerorder WHERE OrderID = @orderId AND CustName = @username
        """)]
    Task<CustomerOrder?> GetByOrderNumberAsync(int orderId, string username);

    [SqlQuery("""
        INSERT INTO CustomerOrder ("Status", TotalPrice, CustName, OrderDate, ShippingAddress)
        VALUES (@status::order_status_enum, @totalPrice, @custName, @orderDate, @shippingAddress) RETURNING OrderID
        """)]
    Task<int> InsertOrderAsync(string status, decimal totalPrice, string custName, DateTime orderDate,
        string shippingAddress, IDbTransaction transaction);

    [SqlUpdate("""
        INSERT INTO customerorderitem (ISBN, OrderNum, Quantity, UnitPrice, Title)
        VALUES (@isbn, @orderNum, @quantity, @unitPrice, @title)
        """)]
    Task InsertItemAsync(string isbn, int orderNum, int quantity, decimal unitPrice, string title,
        IDbTransaction transaction);

    [SqlUpdate("""
        UPDATE CustomerOrder SET "Status" = @status::order_status_enum
        WHERE OrderID = @orderId AND CustName = @custName
        """)]
    Task UpdateStatusAsync(int orderId, string custName, string status);

    [SqlQuery("""
        SELECT ISBN, OrderNum, Quantity, UnitPrice, Title FROM customerorderItem WHERE OrderNum = @orderNum
        """)]
    Task<IReadOnlyList<CustomerOrderItem>> GetItemsAsync(int orderNum);

    [SqlUpdate("DELETE FROM CustomerOrder WHERE OrderID = @orderId AND CustName = @custName")]
    Task DeleteAsync(int orderId, string custName);
}
