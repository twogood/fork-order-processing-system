using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Interfaces.Repositories;
using OrderProcessing.Infrastructure.Data;

namespace OrderProcessing.Infrastructure.Repositories;

public class AdminOrderRepository(IAdminOrderDatabase database) : IAdminOrderRepository
{
    public Task<IReadOnlyList<AdminOrder>> GetAllAsync() => database.GetAllAsync();

    public Task<AdminOrder?> GetByOrderIdAsync(int orderId) => database.GetByOrderIdAsync(orderId);

    public async Task<int> AddAsync(AdminOrder order, List<AdminOrderItem> items)
    {
        using var transaction = database.BeginTransaction();
        try
        {
            var orderId = await database.InsertOrderAsync(order.OrderDate.ToDateTime(TimeOnly.MinValue),
                order.Status.ToString(), order.TotalPrice, order.PubID, order.ConfirmedBy, transaction);
            foreach (var item in items)
                await database.InsertItemAsync(item.ISBN, orderId, item.Quantity, item.UnitPrice, transaction);
            transaction.Commit();
            return orderId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task UpdateStatusAsync(int orderId, string status) => database.UpdateStatusAsync(orderId, status);

    public Task UpdateStatusAndConfirmedByAsync(int orderId, string status, string confirmedBy) =>
        database.UpdateStatusAndConfirmedByAsync(orderId, status, confirmedBy);

    public Task DeleteAsync(int orderId) => database.DeleteAsync(orderId);

    public Task<int> GetOrderCountForBookAsync(int isbn) => database.GetOrderCountForBookAsync(isbn);

    public Task<List<AdminOrderItem>> GetOrderItemsAsync(int orderId) => database.GetItemsAsync(orderId);
}
