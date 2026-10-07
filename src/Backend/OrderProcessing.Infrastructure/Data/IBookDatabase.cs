using System.Data;
using Activout.DatabaseClient;
using Activout.DatabaseClient.Attributes;
using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Models;

namespace OrderProcessing.Infrastructure.Data;

public interface IBookDatabase : IWithTransactions
{
    [SqlQuery("SELECT * FROM book WHERE isbn = @isbn")]
    Task<Book?> GetByIsbnAsync(string isbn);

    [SqlUpdate("""
        INSERT INTO Book (ISBN, Title, PublicationYear, SellingPrice, Quantity, Threshold, Category, PubID)
        VALUES (@isbn, @title, @publicationYear, @sellingPrice, @quantity, @threshold, @category::category_enum, @pubId)
        """)]
    Task InsertBookAsync(string isbn, string title, int publicationYear, decimal sellingPrice, int quantity,
        int threshold, string category, int pubId, IDbTransaction transaction);

    [SqlUpdate("INSERT INTO BookAuthor (ISBN, AuthorName) VALUES (@isbn, @authorName)")]
    Task InsertAuthorAsync(string isbn, string authorName, IDbTransaction transaction);

    [SqlUpdate("""
        UPDATE Book SET Title = @title, PublicationYear = @publicationYear, SellingPrice = @sellingPrice,
        Quantity = @quantity, Threshold = @threshold, Category = @category::category_enum, PubID = @pubId
        WHERE ISBN = @isbn
        """)]
    Task UpdateBookAsync(string isbn, string title, int publicationYear, decimal sellingPrice, int quantity,
        int threshold, string category, int pubId, IDbTransaction transaction);

    [SqlUpdate("DELETE FROM bookauthor WHERE isbn = @isbn")]
    Task DeleteAuthorsAsync(string isbn, IDbTransaction transaction);

    [SqlUpdate("DELETE FROM book WHERE isbn = @isbn")]
    Task DeleteAsync(string isbn);

    [SqlQuery("SELECT ISBN FROM book WHERE isbn = @isbn")]
    Task<string?> FindIsbnAsync(string isbn);

    [SqlQuery("""
        SELECT b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold,
               b.Category AS CategoryName, p.PubName AS PublisherName,
               COALESCE(STRING_AGG(ba.AuthorName, ', '), '') AS AuthorNames
        FROM book b JOIN Publisher p ON b.PubID = p.PubID LEFT JOIN BookAuthor ba ON b.ISBN = ba.ISBN
        WHERE b.ISBN = @isbn
        GROUP BY b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold, b.Category, p.PubName
        """)]
    Task<BookDetailsReadModel?> GetDetailsAsync(string isbn);

    [SqlQuery("""
        SELECT b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold,
               b.Category AS CategoryName, p.PubName AS PublisherName,
               COALESCE(STRING_AGG(ba.AuthorName, ', '), '') AS AuthorNames
        FROM book b JOIN Publisher p ON b.PubID = p.PubID LEFT JOIN BookAuthor ba ON b.ISBN = ba.ISBN
        WHERE b.ISBN = ANY(@isbns)
        GROUP BY b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold, b.Category, p.PubName
        """)]
    Task<IEnumerable<BookDetailsReadModel>> GetDetailsAsync(string[] isbns);

    [SqlQuery("""
        SELECT b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold,
               b.Category AS CategoryName, p.PubName AS PublisherName,
               COALESCE(STRING_AGG(ba.AuthorName, ', '), '') AS AuthorNames
        FROM Book b JOIN Publisher p ON b.PubID = p.PubID LEFT JOIN BookAuthor ba ON b.ISBN = ba.ISBN
        GROUP BY b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold, b.Category, p.PubName
        """)]
    Task<IEnumerable<BookDetailsReadModel>> GetAllDetailsAsync();

    [SqlQuery("""
        SELECT b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold,
               b.Category AS CategoryName, p.PubName AS PublisherName,
               COALESCE(STRING_AGG(ba.AuthorName, ', '), '') AS AuthorNames
        FROM Book b JOIN Publisher p ON b.PubID = p.PubID LEFT JOIN BookAuthor ba ON b.ISBN = ba.ISBN
        WHERE b.Quantity < b.Threshold
        GROUP BY b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold, b.Category, p.PubName
        """)]
    Task<IEnumerable<BookDetailsReadModel>> GetBelowThresholdAsync();

    [SqlQuery("""
        SELECT b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold,
               b.Category AS CategoryName, p.PubName AS PublisherName,
               COALESCE(STRING_AGG(ba.AuthorName, ', '), '') AS AuthorNames
        FROM Book b JOIN Publisher p ON b.PubID = p.PubID LEFT JOIN BookAuthor ba ON b.ISBN = ba.ISBN
        WHERE (@search IS NULL OR REPLACE(b.ISBN, '-', '') ILIKE '%' || REPLACE(@search, '-', '') || '%'
               OR b.Title ILIKE '%' || @search || '%')
          AND (@category IS NULL OR b.Category = @category::category_enum)
          AND (@author IS NULL OR ba.AuthorName ILIKE '%' || @author || '%')
          AND (@publisher IS NULL OR p.PubName ILIKE '%' || @publisher || '%')
        GROUP BY b.ISBN, b.Title, b.PublicationYear, b.SellingPrice, b.Quantity, b.Threshold, b.Category, p.PubName
        """)]
    Task<IEnumerable<BookDetailsReadModel>> SearchAsync(string? search, string? category, string? author,
        string? publisher);

    [SqlUpdate("UPDATE Book SET Quantity = Quantity + @quantityChange WHERE ISBN = @isbn")]
    Task UpdateQuantityAsync(string isbn, int quantityChange);

    [SqlQuery("""
        SELECT ISBN, Quantity, Title FROM Book WHERE ISBN = ANY(@isbns) FOR UPDATE
        """)]
    Task<IEnumerable<BookStockRow>> LockBooksAsync(string[] isbns, IDbTransaction transaction);

    [SqlUpdate("UPDATE Book SET Quantity = @quantity WHERE ISBN = @isbn")]
    Task SetQuantityAsync(string isbn, int quantity, IDbTransaction transaction);
}

public record BookStockRow(string ISBN, int Quantity, string Title);
