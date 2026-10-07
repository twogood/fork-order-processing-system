using Npgsql;
using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Interfaces.Repositories;
using OrderProcessing.Infrastructure.Data;

namespace OrderProcessing.Infrastructure.Repositories;

public class CreditCardRepository(ICreditCardDatabase database) : ICreditCardRepository
{
    public Task<CreditCard?> GetByNumberAsync(long cardNumber) => database.GetByNumberAsync(cardNumber);

    public Task<IEnumerable<CreditCard>> GetUserCardsAsync(string username) => database.GetUserCardsAsync(username);

    public async Task AddAsync(CreditCard card, string username)
    {
        using var transaction = database.BeginTransaction();
        try
        {
            await database.InsertCardAsync(card.CardNumber, card.ExpiryDate.ToDateTime(TimeOnly.MinValue),
                card.CardholderName, transaction);
            await database.InsertHolderAsync(card.CardNumber, username, transaction);
            transaction.Commit();
        }
        catch (PostgresException exception)
        {
            transaction.Rollback();
            throw PostgresExceptionTranslator.Translate(exception);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task DeleteAsync(long cardNumber, string username)
    {
        using var transaction = database.BeginTransaction();
        try
        {
            await database.DeleteHolderAsync(cardNumber, username, transaction);
            if (await database.CountHoldersAsync(cardNumber, transaction) == 0)
                await database.DeleteCardAsync(cardNumber, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> ValidateCreditCardAsync(long cardNumber, DateTime expiryDate) =>
        await database.CountValidCardsAsync(cardNumber, expiryDate.Date) > 0;
}
