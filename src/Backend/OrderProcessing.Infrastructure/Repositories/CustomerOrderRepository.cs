using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Interfaces.Repositories;
using OrderProcessing.Domain.ValueObjects;
using OrderProcessing.Infrastructure.Data;

namespace OrderProcessing.Infrastructure.Repositories;

public class CustomerOrderRepository(ICustomerOrderDatabase database) : ICustomerOrderRepository
{
    public async Task<IReadOnlyList<CustomerOrder>> GetByUsernameAsync(string username) =>
        (await database.GetByUsernameAsync(username)).ToList();

    public Task<CustomerOrder?> GetByOrderNumberAsync(int orderNumber, string username) =>
        database.GetByOrderNumberAsync(orderNumber, username);

    public async Task<int> AddAsync(CustomerOrder order, List<CustomerOrderItem> items)
    {
        using var transaction = database.BeginTransaction();
        try
        {
            var orderId = await database.InsertOrderAsync(order.Status.ToString(), order.TotalPrice, order.Username,
                order.OrderDate.ToDateTime(TimeOnly.MinValue), order.ShippingAddress, transaction);
            foreach (var item in items)
                await database.InsertItemAsync(item.ISBN, orderId, item.Quantity, item.UnitPrice, item.Title, transaction);
            transaction.Commit();
            return orderId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task UpdateStatusAsync(int orderNumber, string username) =>
        database.UpdateStatusAsync(orderNumber, username, OrderStatus.Canceled.ToString());

    public async Task<IReadOnlyList<CustomerOrderItem>> GetOrderItemsAsync(int orderNumber) =>
        (await database.GetItemsAsync(orderNumber)).ToList();

    public Task DeleteAsync(int orderNumber, string username) => database.DeleteAsync(orderNumber, username);
}
