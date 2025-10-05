using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Models
{
    [PrimaryKey(nameof(ProductId),nameof(OrderId))]
    public class OrderItem
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int Count { get; set; }

        public decimal TotalPrice  { get; set; }
    }
}
