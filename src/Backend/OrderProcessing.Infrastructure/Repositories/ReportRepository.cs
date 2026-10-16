using OrderProcessing.Domain.Interfaces.Repositories;
using OrderProcessing.Domain.Models;
using OrderProcessing.Infrastructure.Data;

namespace OrderProcessing.Infrastructure.Repositories;

public class ReportRepository(IReportDatabase database) : IReportRepository
{
    public async Task<SalesReportReadModel> GetTotalSalesPreviousMonthAsync()
    {
        var sales = await database.GetPreviousMonthSalesAsync();
        return new SalesReportReadModel("Previous Month", await database.GetPreviousMonthTotalAsync(), sales.Count());
    }

    public async Task<SalesReportReadModel> GetTotalSalesByDateAsync(DateOnly date)
    {
        var value = date.ToDateTime(TimeOnly.MinValue);
        var sales = await database.GetSalesByDateAsync(value);
        return new SalesReportReadModel(date.ToString("yyyy-MM-dd"), await database.GetTotalByDateAsync(value),
            sales.Count());
    }

    public Task<IEnumerable<TopCustomerReadModel>> GetTop5CustomersAsync() => database.GetTop5CustomersAsync();

    public Task<IEnumerable<TopSellingBookReadModel>> GetTop10SellingBooksAsync() =>
        database.GetTop10SellingBooksAsync();

    public Task<BookReplenishmentCountReadModel?> GetBookReplenishmentCountAsync(string isbn) =>
        database.GetBookReplenishmentCountAsync(isbn);
}
