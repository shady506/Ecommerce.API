
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Ecommerce.API.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly IProductRepository _productRepository;

        public ValuesController(IRepository<Category> categoryRepository, IRepository<Brand> brandRepository,IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _productRepository = productRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(ProductFilterRequest productFilterRequest, int page = 1)
        {
            const double discount = 50;
            var products = (await _productRepository.GetAsync(includes: [e=>e.Category,e=>e.Brand])).AsQueryable();

            // Filter
            if (productFilterRequest.ProductName is not null)
            {
                products = products.Where(e => e.Name.Contains(productFilterRequest.ProductName));
            }

            if (productFilterRequest.MinPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * (e.Discount / 100) >= productFilterRequest.MinPrice);

            }

            if (productFilterRequest.MaxPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * (e.Discount / 100) <= productFilterRequest.MaxPrice);
       
            }

            if (productFilterRequest.CategoryId is not null)
            {
                products = products.Where(e => e.CategoryId == productFilterRequest.CategoryId);
            }

            if (productFilterRequest.IsHot)
            {
                products = products.Where(e => e.Discount > discount);
            }

            // Pagination
            double totalPages = Math.Ceiling(products.Count() / 8.0); // 3.1 => 4
            int currentPage = page;

           

            products = products.Skip((page - 1) * 8).Take(8);

            // Returned Data
            var categories =  await _categoryRepository.GetAsync();
            //ViewData["Categories"] = categories;

            return Ok(new
            {
                products,
                productFilterRequest.ProductName,
                productFilterRequest.MinPrice,
                productFilterRequest.MaxPrice,
                productFilterRequest.CategoryId,
                productFilterRequest.IsHot,
                totalPages,
                currentPage,
                categories


            });
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> Details([FromRoute] int id)
        {
            var product = await _productRepository.GetOneAsync(e => e.Id == id, includes: [e => e.Category,e=>e.Brand]);

            if (product is null)
                return NotFound();

            // Update Traffic
            ++product.Traffic;
            await _productRepository.CommitAsync();

            // Related Products
            var relatedProducts = (await _productRepository.GetAsync(e => e.CategoryId == product.CategoryId && e.Id != product.Id,includes: [e=>e.Category])).Skip(0).Take(4);

            // Top Traffic
            var topTraffic = (await _productRepository.GetAsync(e => e.Id != product.Id,includes: [e => e.Category])).OrderByDescending(e => e.Traffic).Skip(0).Take(4);

            // Similar Products
            var similarProducts = (await _productRepository.GetAsync(e => e.Name.Contains(product.Name) && e.Id != product.Id,includes: [e => e.Category])).Skip(0).Take(4);

            // Return Data
             ProductWithRelatedRespnse productWithRelatedRespnse = new()
            {
                Product = product,
                RelatedProducts = relatedProducts.ToList(),
                TopTraffic = topTraffic.ToList(),
                SimilarProducts = similarProducts.ToList()
            };

            return Ok(productWithRelatedRespnse);
        }
    }
}
