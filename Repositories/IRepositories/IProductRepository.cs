using System.Linq.Expressions;

namespace  Ecommerce.API.Repositories.IRepositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task AddRangeAsync(List<Product> products);
    }
}
