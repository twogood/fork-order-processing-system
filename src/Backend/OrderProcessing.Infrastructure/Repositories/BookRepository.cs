using Npgsql;
using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Exceptions;
using OrderProcessing.Domain.Interfaces.Repositories;
using OrderProcessing.Domain.Models;
using OrderProcessing.Infrastructure.Data;

namespace OrderProcessing.Infrastructure.Repositories;

public class BookRepository(IBookDatabase database) : IBookRepository
{
    public Task<Book?> GetByISBNAsync(string isbn) => database.GetByIsbnAsync(isbn);

    public async Task AddAsync(Book book)
    {
        using var transaction = database.BeginTransaction();
        try
        {
            await database.InsertBookAsync(book.ISBN, book.Title, book.PublicationYear, book.SellingPrice,
                book.Quantity, book.Threshold, book.Category.ToString(), book.PubID, transaction);
            foreach (var author in book.Authors)
                await database.InsertAuthorAsync(book.ISBN, author.AuthorName, transaction);
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

    public async Task UpdateAsync(Book book)
    {
        using var transaction = database.BeginTransaction();
        try
        {
            await database.UpdateBookAsync(book.ISBN, book.Title, book.PublicationYear, book.SellingPrice,
                book.Quantity, book.Threshold, book.Category.ToString(), book.PubID, transaction);
            if (book.Authors?.Any() == true)
            {
                await database.DeleteAuthorsAsync(book.ISBN, transaction);
                foreach (var author in book.Authors)
                    await database.InsertAuthorAsync(book.ISBN, author.AuthorName, transaction);
            }
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

    public async Task DeleteAsync(string isbn)
    {
        try { await database.DeleteAsync(isbn); }
        catch (PostgresException exception) { throw PostgresExceptionTranslator.Translate(exception); }
    }

    public async Task<bool> ExistsAsync(string isbn) => await database.FindIsbnAsync(isbn) is not null;

    public Task<BookDetailsReadModel?> GetBookDetailsAsync(string isbn) => database.GetDetailsAsync(isbn);

    public async Task<Dictionary<string, BookDetailsReadModel>> GetBookDetailsAsync(IEnumerable<string> isbns) =>
        (await database.GetDetailsAsync(isbns.ToArray())).ToDictionary(book => book.ISBN);

    public Task<IEnumerable<BookDetailsReadModel>> GetAllBookDetailsAsync() => database.GetAllDetailsAsync();

    public Task<IEnumerable<BookDetailsReadModel>> GetBooksBelowStockThresholdAsync() => database.GetBelowThresholdAsync();

    public Task<IEnumerable<BookDetailsReadModel>> SearchBooksAsync(BookSearchFilter filter) =>
        database.SearchAsync(Normalize(filter.Search), Normalize(filter.Category?.ToString()),
            Normalize(filter.Author), Normalize(filter.Publisher));

    public Task UpdateBookQuantityAsync(string isbn, int quantityChange) =>
        database.UpdateQuantityAsync(isbn, quantityChange);

    public async Task UpdateBookQuantitiesAsync(Dictionary<string, int> quantityChanges)
    {
        if (quantityChanges.Count == 0)
            return;

        using var transaction = database.BeginTransaction();
        try
        {
            var rows = (await database.LockBooksAsync(quantityChanges.Keys.ToArray(), transaction))
                .ToDictionary(row => row.ISBN);
            var insufficient = new List<InsufficientStockException.InsufficientItem>();

            foreach (var (isbn, change) in quantityChanges)
            {
                if (!rows.TryGetValue(isbn, out var row))
                    throw new InvalidOperationException($"Book not found for ISBN {isbn}");
                if (row.Quantity + change < 0)
                    insufficient.Add(new InsufficientStockException.InsufficientItem(isbn, row.Quantity, row.Title));
            }

            if (insufficient.Count > 0)
                throw new InsufficientStockException(insufficient);

            foreach (var (isbn, change) in quantityChanges)
                await database.SetQuantityAsync(isbn, rows[isbn].Quantity + change, transaction);
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

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
