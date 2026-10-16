using Activout.DatabaseClient.Attributes;
using OrderProcessing.Domain.Entities;

namespace OrderProcessing.Infrastructure.Data;

public interface IShoppingCartDatabase
{
    [SqlQuery("SELECT CartId, CustName AS Username FROM ShoppingCart WHERE CustName = @username")]
    Task<CartInfo?> GetCartAsync(string username);

    [SqlQuery("SELECT CartId, ISBN, Quantity, UnitPrice FROM CartItem WHERE CartId = @cartId")]
    Task<IEnumerable<CartItem>> GetCartItemsAsync(int cartId);

    [SqlQuery("SELECT CartId FROM ShoppingCart WHERE custname = @username")]
    Task<int?> GetCartIdAsync(string username);

    [SqlQuery("""INSERT INTO ShoppingCart (custname) VALUES (@username) RETURNING cartid""")]
    Task<int> CreateCartAsync(string username);

    [SqlUpdate("""
        INSERT INTO CartItem (ISBN, CartId, Quantity, UnitPrice)
        VALUES (@isbn, @cartId, @quantity, @unitPrice)
        ON CONFLICT (ISBN, CartId) DO UPDATE SET Quantity = CartItem.Quantity + @quantity
        """)]
    Task AddCartItemAsync(string isbn, int cartId, int quantity, decimal unitPrice);

    [SqlUpdate("UPDATE CartItem SET quantity = @quantity WHERE cartid = @cartId AND isbn = @isbn")]
    Task<int> UpdateCartItemAsync(int cartId, string isbn, int quantity);

    [SqlUpdate("DELETE FROM CartItem WHERE CartId = @cartId AND ISBN = @isbn")]
    Task RemoveCartItemAsync(int cartId, string isbn);

    [SqlUpdate("DELETE FROM CartItem WHERE CartId = @cartId")]
    Task ClearCartAsync(int cartId);

    [SqlQuery("SELECT COALESCE(SUM(quantity), 0) FROM CartItem WHERE cartid = @cartId")]
    Task<int> GetCartItemCountAsync(int cartId);
}

public record CartInfo(int CartId, string Username);
