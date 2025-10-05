using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;



namespace Ecommerce.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private IProductRepository _productRepository;// = new ProductRepository();
        private IRepository<Brand> _brandRepository;// = new Repository<Brand>();
        private IRepository<Category> _categoryRepository;// = new Repository<Category>();
        public ProductsController(IProductRepository productRepository,
          IRepository<Brand> brandRepository,
          IRepository<Category> categoryRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAsync(includes: [e => e.Category]);

            var ProductResponse = products.Adapt<List<ProductResponse>>();

            return Ok(ProductResponse);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {

            var product = await _productRepository.GetOneAsync(e => e.Id == id, includes: [e => e.Category]);

            return Ok(product.Adapt<ProductResponse>());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest productCreateRequest)
        {
            if (productCreateRequest.MainImg is not null && productCreateRequest.MainImg.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productCreateRequest.MainImg.FileName);
                // 0924fdsfs-d429-fskdf-jsd230-423.png

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                // Save Img in wwwroot
                using (var stream = System.IO.File.Create(filePath))
                {
                    await productCreateRequest.MainImg.CopyToAsync(stream);
                }

                // Sava img name in DB
                var product = productCreateRequest.Adapt<Product>();
                product.MainImg = fileName;

                // Save in DB
               var ProductReturned = await _productRepository.CreateAsync(product);
                await _productRepository.CommitAsync();


                //return Created($"{Request.Scheme}://{Request.Host}/api/admin/products/{ProductReturned.Id}", new 
                //{
                //    msg ="Created Product Successfully"
                //});

                return CreatedAtRoute(nameof(Details), new { id = ProductReturned.Id }, new
                {
                    msg = "Created Product Successfully"
                });
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id,[FromForm]ProductUpdateRequest productUpdateRequest)
        {
            var productInDB = await _productRepository.GetOneAsync(e => e.Id == id, tracked: false);
            if (productInDB is null)
                return BadRequest();
            var product = productUpdateRequest.Adapt<Product>();
            product.Id = id;


            if (productUpdateRequest.MainImg is not null && productUpdateRequest.MainImg.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(productUpdateRequest.MainImg.FileName);
                // 0924fdsfs-d429-fskdf-jsd230-423.png

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                // Save Img in wwwroot
                using (var stream = System.IO.File.Create(filePath))
                {
                    await productUpdateRequest.MainImg.CopyToAsync(stream);
                }

                // Delete old img from wwwroot
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", productInDB.MainImg);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Update img name in DB
                
                product.MainImg = fileName;
            }
            else
            {
                product.MainImg = productInDB.MainImg;
            }

            // Update in DB
            _productRepository.Update(product);
            await _productRepository.CommitAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetOneAsync(e => e.Id == id);

            if (product is null)
                NotFound();

            // Delete old img from wwwroot
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", product.MainImg);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }

            // Remove in DB
            _productRepository.Delete(product);
            await _productRepository.CommitAsync();

            return NoContent();
        }
    }
}
