using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Interfaces.Repositories;
using OrderProcessing.Domain.Models;
using OrderProcessing.Infrastructure.Data;

namespace OrderProcessing.Infrastructure.Repositories;

public class ShoppingCartRepository(IShoppingCartDatabase database) : IShoppingCartRepository
{
    public async Task<ShoppingCartReadModel?> GetCartByUsernameAsync(string username)
    {
        var cart = await database.GetCartAsync(username);
        if (cart is null)
            return null;

        var items = (await database.GetCartItemsAsync(cart.CartId))
            .Select(item => new CartItemReadModel(item.ISBN, item.Quantity, item.UnitPrice)).ToList();
        return new ShoppingCartReadModel(cart.CartId, cart.Username, items);
    }

    public async Task<int> CreateCartAsync(string username) =>
        await database.GetCartIdAsync(username) ?? await database.CreateCartAsync(username);

    public async Task<ShoppingCartReadModel> GetOrCreateCartAsync(string username) =>
        await GetCartByUsernameAsync(username) ??
        new ShoppingCartReadModel(await CreateCartAsync(username), username, []);

    public Task AddCartItemAsync(int cartId, CartItem cartItem) =>
        database.AddCartItemAsync(cartItem.ISBN, cartId, cartItem.Quantity, cartItem.UnitPrice);

    public Task<int> UpdateCartItemAsync(int cartId, CartItemReadModel cartItem) =>
        database.UpdateCartItemAsync(cartId, cartItem.ISBN, cartItem.Quantity);

    public Task RemoveCartItemAsync(int cartId, string isbn) => database.RemoveCartItemAsync(cartId, isbn);

    public Task ClearCartAsync(int cartId) => database.ClearCartAsync(cartId);

    public async Task<int> GetCartItemCountAsync(string username)
    {
        var cartId = await database.GetCartIdAsync(username);
        return cartId is null ? 0 : await database.GetCartItemCountAsync(cartId.Value);
    }
}
