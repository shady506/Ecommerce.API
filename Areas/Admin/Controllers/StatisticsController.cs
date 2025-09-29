using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ecommerce.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public StatisticsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
           var products = (await _productRepository.GetAsync(includes: [e=>e.Category])).GroupBy(e=>e.CategoryId).Select(e => new 
            {
                e.Key,
                count = e.Count(),
                avg = e.Average(e=>e.Price).ToString("c"),
                sum = e.Sum(e=>e.Price).ToString("c")
            });
            
            return Ok(products);
        }


    }
}
