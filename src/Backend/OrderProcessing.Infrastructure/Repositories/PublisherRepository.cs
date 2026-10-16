using OrderProcessing.Domain.Entities;
using OrderProcessing.Domain.Interfaces.Repositories;
using OrderProcessing.Infrastructure.Data;

namespace OrderProcessing.Infrastructure.Repositories;

public class PublisherRepository(IPublisherDatabase database) : IPublisherRepository
{
    public Task<IEnumerable<Publisher>> GetAllAsync() => database.GetAllAsync();

    public Task<Publisher?> GetByIdAsync(int id) => database.GetByIdAsync(id);
}
