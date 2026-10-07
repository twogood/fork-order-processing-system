using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Interfaces.Repositories;
using OrderProcessing.Domain.ValueObjects;
using OrderProcessing.Infrastructure.Data;

namespace OrderProcessing.Infrastructure.Repositories;

public class UserRepository(IUserDatabase database) : IUserRepository
{
    public async Task<User?> GetByUserNameAsync(string username)
    {
        var row = await database.GetByUsernameAsync(username);
        return row is null ? null : ToUser(row);
    }

    public Task AddAsync(User user) => database.AddAsync(user.Username, user.PasswordHash, user.FirstName,
        user.LastName, user.Email, user.PhoneNumber, user.Address, user.Role.ToString());

    public Task UpdateAsync(User user) => database.UpdateAsync(user.Username, user.PasswordHash, user.FirstName,
        user.LastName, user.Email, user.PhoneNumber, user.Address, user.Role.ToString());

    public Task DeleteAsync(string username) => database.DeleteAsync(username);

    public async Task<IReadOnlyList<User>> GetAllCustomersAsync() =>
        (await database.GetAllCustomersAsync()).Select(ToUser).ToList();

    public async Task<IReadOnlyList<User>> SearchCustomersAsync(string? username, string? firstName,
        string? lastName, string? email) =>
        (await database.SearchCustomersAsync(Normalize(username), Normalize(firstName), Normalize(lastName),
            Normalize(email))).Select(ToUser).ToList();

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static User ToUser(UserRow row)
    {
        var role = UserTypes.Customer;
        if (!string.IsNullOrWhiteSpace(row.Role))
            Enum.TryParse(row.Role, true, out role);

        return new User(row.Username, row.Email, row.PhoneNumber, row.FirstName, row.LastName,
            row.Password, role, row.ShipAddress ?? string.Empty);
    }
}
