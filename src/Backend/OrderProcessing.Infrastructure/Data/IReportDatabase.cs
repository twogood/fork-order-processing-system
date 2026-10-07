using Activout.DatabaseClient.Attributes;
using OrderProcessing.Domain.Models;

namespace OrderProcessing.Infrastructure.Data;

public interface IReportDatabase
{
    [SqlQuery("""
        SELECT OrderID, OrderDate, TotalPrice FROM customerorder
        WHERE "Status" = 'Confirmed'::order_status_enum
          AND OrderDate >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')
          AND OrderDate < DATE_TRUNC('month', CURRENT_DATE)
        """)]
    Task<IEnumerable<SaleRow>> GetPreviousMonthSalesAsync();

    [SqlQuery("""
        SELECT COALESCE(SUM(TotalPrice), 0) FROM customerorder
        WHERE "Status" = 'Confirmed'::order_status_enum
          AND OrderDate >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')
          AND OrderDate < DATE_TRUNC('month', CURRENT_DATE)
        """)]
    Task<decimal> GetPreviousMonthTotalAsync();

    [SqlQuery("""
        SELECT OrderID, OrderDate, TotalPrice FROM customerorder
        WHERE "Status" = 'Confirmed'::order_status_enum AND OrderDate = @date
        """)]
    Task<IEnumerable<SaleRow>> GetSalesByDateAsync(DateTime date);

    [SqlQuery("""
        SELECT COALESCE(SUM(TotalPrice), 0) FROM customerorder
        WHERE "Status" = 'Confirmed'::order_status_enum AND OrderDate = @date
        """)]
    Task<decimal> GetTotalByDateAsync(DateTime date);

    [SqlQuery("""
        SELECT co.CustName AS CustomerName, SUM(co.TotalPrice) AS TotalSpent, u.Email AS Email
        FROM customerorder co JOIN "User" u ON co.CustName = u.Username
        WHERE co."Status" = 'Confirmed'::order_status_enum
          AND co.OrderDate >= CURRENT_DATE - INTERVAL '3 months' AND co.OrderDate <= CURRENT_DATE
        GROUP BY co.CustName, u.Email ORDER BY TotalSpent DESC LIMIT 5
        """)]
    Task<IEnumerable<TopCustomerReadModel>> GetTop5CustomersAsync();

    [SqlQuery("""
        SELECT b.ISBN, b.Title, SUM(oi.Quantity) AS TotalCopiesSold
        FROM customerorderItem oi JOIN Book b ON b.ISBN = oi.ISBN
        JOIN CustomerOrder o ON o.OrderID = oi.OrderNum
        WHERE o."Status" = 'Confirmed'::order_status_enum
          AND o.OrderDate >= CURRENT_DATE - INTERVAL '3 months' AND o.OrderDate <= CURRENT_DATE
        GROUP BY b.ISBN, b.Title ORDER BY TotalCopiesSold DESC LIMIT 10
        """)]
    Task<IEnumerable<TopSellingBookReadModel>> GetTop10SellingBooksAsync();

    [SqlQuery("""
        SELECT b.ISBN, b.Title, COUNT(DISTINCT aoi.OrderNum) AS TimesOrderedFromPublisher,
               COALESCE(SUM(aoi.Quantity), 0) AS TotalQuantityOrdered
        FROM book b LEFT JOIN AdminOrderItem aoi ON b.ISBN = aoi.ISBN
        WHERE b.ISBN = @isbn GROUP BY b.ISBN, b.Title
        """)]
    Task<BookReplenishmentCountReadModel?> GetBookReplenishmentCountAsync(string isbn);
}

public record SaleRow(int OrderId, DateTime OrderDate, decimal TotalPrice);
