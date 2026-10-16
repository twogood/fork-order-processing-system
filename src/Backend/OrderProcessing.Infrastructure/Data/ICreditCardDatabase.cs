using System.Data;
using Activout.DatabaseClient;
using Activout.DatabaseClient.Attributes;
using OrderProcessing.Domain.Entities;

namespace OrderProcessing.Infrastructure.Data;

public interface ICreditCardDatabase : IWithTransactions
{
    [SqlQuery("SELECT CardNumber, ExpiryDate, CardholderName FROM creditcard WHERE CardNumber = @cardNumber")]
    Task<CreditCard?> GetByNumberAsync(long cardNumber);

    [SqlQuery("""
        SELECT cc.CardNumber, cc.ExpiryDate, cc.CardholderName FROM creditcard cc
        INNER JOIN cardholder ch ON ch.CardNumber = cc.CardNumber WHERE ch.Username = @username
        """)]
    Task<IEnumerable<CreditCard>> GetUserCardsAsync(string username);

    [SqlUpdate("""
        INSERT INTO creditcard (cardnumber, expirydate, cardholdername)
        VALUES (@cardNumber, @expiryDate, @cardholderName)
        """)]
    Task InsertCardAsync(long cardNumber, DateTime expiryDate, string cardholderName, IDbTransaction transaction);

    [SqlUpdate("INSERT INTO cardholder (cardnumber, username) VALUES (@cardNumber, @username)")]
    Task InsertHolderAsync(long cardNumber, string username, IDbTransaction transaction);

    [SqlUpdate("DELETE FROM cardholder WHERE cardnumber = @cardNumber AND username = @username")]
    Task DeleteHolderAsync(long cardNumber, string username, IDbTransaction transaction);

    [SqlQuery("SELECT COUNT(*) FROM cardholder WHERE cardnumber = @cardNumber")]
    Task<int> CountHoldersAsync(long cardNumber, IDbTransaction transaction);

    [SqlUpdate("DELETE FROM creditcard WHERE cardnumber = @cardNumber")]
    Task DeleteCardAsync(long cardNumber, IDbTransaction transaction);

    [SqlQuery("""
        SELECT COUNT(*) FROM creditcard WHERE cardnumber = @cardNumber AND expirydate >= @expiryDate
        """)]
    Task<int> CountValidCardsAsync(long cardNumber, DateTime expiryDate);
}
