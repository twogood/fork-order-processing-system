using Activout.DatabaseClient.Attributes;

namespace OrderProcessing.Infrastructure.Data;

public interface IUserDatabase
{
    [SqlQuery("""
        SELECT Username, "Password", FirstName, LastName, ShipAddress, Email, PhoneNumber, "Role"
        FROM "User" WHERE Username = @username
        """)]
    Task<UserRow?> GetByUsernameAsync(string username);

    [SqlUpdate("""
        INSERT INTO "User" (Username, "Password", FirstName, LastName, Email, PhoneNumber, ShipAddress, "Role")
        VALUES (@username, @passwordHash, @firstName, @lastName, @email, @phoneNumber, @address, @role::role_enum)
        """)]
    Task AddAsync(string username, string passwordHash, string firstName, string lastName,
        string email, string phoneNumber, string address, string role);

    [SqlUpdate("""
        UPDATE "User"
        SET Email = @email, PhoneNumber = @phoneNumber, FirstName = @firstName, LastName = @lastName,
            ShipAddress = @address, "Password" = @passwordHash, "Role" = @role::role_enum
        WHERE Username = @username
        """)]
    Task UpdateAsync(string username, string passwordHash, string firstName, string lastName,
        string email, string phoneNumber, string address, string role);

    [SqlUpdate("""DELETE FROM "User" WHERE Username = @username""")]
    Task DeleteAsync(string username);

    [SqlQuery("""
        SELECT Username, "Password", FirstName, LastName, ShipAddress, Email, PhoneNumber, "Role"
        FROM "User" WHERE "Role" = 'Customer' ORDER BY Username
        """)]
    Task<IEnumerable<UserRow>> GetAllCustomersAsync();

    [SqlQuery("""
        SELECT Username, "Password", FirstName, LastName, ShipAddress, Email, PhoneNumber, "Role"
        FROM "User"
        WHERE "Role" = 'Customer'
          AND (@username IS NULL OR Username ILIKE '%' || @username || '%')
          AND (@firstName IS NULL OR FirstName ILIKE '%' || @firstName || '%')
          AND (@lastName IS NULL OR LastName ILIKE '%' || @lastName || '%')
          AND (@email IS NULL OR Email ILIKE '%' || @email || '%')
        ORDER BY Username
        """)]
    Task<IEnumerable<UserRow>> SearchCustomersAsync(string? username, string? firstName,
        string? lastName, string? email);
}

public record UserRow(string Username, string Password, string FirstName, string LastName,
    string? ShipAddress, string Email, string PhoneNumber, string? Role);
