using System.Data;
using Activout.DatabaseClient;
using Activout.DatabaseClient.Attributes;
using OrderProcessing.Domain.Entities;

namespace OrderProcessing.Infrastructure.Data;

public interface IAdminOrderDatabase : IWithTransactions
{
    [SqlQuery("""
        SELECT OrderId, OrderDate, "Status", TotalPrice, PubID, ConfirmedBy
        FROM adminorder ORDER BY OrderDate DESC, OrderId DESC
        """)]
    Task<IEnumerable<AdminOrder>> GetAllAsync();

    [SqlQuery("""
        SELECT OrderId, OrderDate, "Status", TotalPrice, PubID, ConfirmedBy
        FROM adminorder WHERE OrderId = @orderId
        """)]
    Task<AdminOrder?> GetByOrderIdAsync(int orderId);

    [SqlQuery("""
        INSERT INTO AdminOrder (OrderDate, "Status", TotalPrice, PubID, ConfirmedBy)
        VALUES (@orderDate, @status::order_status_enum, @totalPrice, @pubId, @confirmedBy) RETURNING OrderId
        """)]
    Task<int> InsertOrderAsync(DateTime orderDate, string status, decimal totalPrice, int pubId,
        string? confirmedBy, IDbTransaction transaction);

    [SqlUpdate("""
        INSERT INTO adminorderitem (ISBN, OrderNum, Quantity, UnitPrice)
        VALUES (@isbn, @orderNum, @quantity, @unitPrice)
        """)]
    Task InsertItemAsync(string isbn, int orderNum, int quantity, decimal unitPrice, IDbTransaction transaction);

    [SqlUpdate("UPDATE AdminOrder SET \"Status\" = @status::order_status_enum WHERE OrderId = @orderId")]
    Task UpdateStatusAsync(int orderId, string status);

    [SqlUpdate("""
        UPDATE AdminOrder SET "Status" = @status::order_status_enum, ConfirmedBy = @confirmedBy
        WHERE OrderId = @orderId
        """)]
    Task UpdateStatusAndConfirmedByAsync(int orderId, string status, string confirmedBy);

    [SqlUpdate("DELETE FROM AdminOrder WHERE OrderId = @orderId")]
    Task DeleteAsync(int orderId);

    [SqlQuery("SELECT COUNT(DISTINCT OrderNum) FROM adminorderitem WHERE ISBN = @isbn")]
    Task<int> GetOrderCountForBookAsync(int isbn);

    [SqlQuery("SELECT ISBN, Quantity, UnitPrice FROM adminorderitem WHERE OrderNum = @orderId")]
    Task<IEnumerable<AdminOrderItem>> GetItemsAsync(int orderId);
}
