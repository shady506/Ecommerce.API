using System.Linq.Expressions;

namespace  Ecommerce.API.Repositories.IRepositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task AddRangeAsync(List<OrderItem> orderItems);
    }
}
