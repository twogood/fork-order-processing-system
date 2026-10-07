using Activout.DatabaseClient.Attributes;
using OrderProcessing.Domain.Entities;

namespace OrderProcessing.Infrastructure.Data;

public interface IPublisherDatabase
{
    [SqlQuery("SELECT * FROM publisher ORDER BY pubname")]
    Task<IEnumerable<Publisher>> GetAllAsync();

    [SqlQuery("SELECT * FROM publisher WHERE pubid = @id")]
    Task<Publisher?> GetByIdAsync(int id);
}
