using  Ecommerce.API.Repositories.IRepositories;

namespace  Ecommerce.API.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _context;// = new();

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(List<Product> products)
        {
            await _context.Products.AddRangeAsync(products);
        }

    }
}
