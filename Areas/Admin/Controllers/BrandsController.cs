using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IRepository<Brand> _brandRepository;

        public BrandsController(IRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var brands = await _brandRepository.GetAsync();

            return Ok(brands);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id);

            if (brand is null)
                return NotFound();

            return Ok(brand);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BrandRequest brandRequest)
        {
            
            await _brandRepository.CreateAsync(brandRequest.Adapt<Brand>());
            await _brandRepository.CommitAsync();

            return Ok(new 
            {
                msg = "Add Brand Successfully"
            });

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id ,BrandRequest brandRequest)
        {
            var brandInDB = await _brandRepository.GetOneAsync(e=>e.Id==id);
            if (brandInDB is null)
                return NotFound();

            brandInDB.Name = brandRequest.Name;
            brandRequest.Description = brandInDB.Description;
            brandRequest.Status = brandInDB.Status;
            await _brandRepository.CommitAsync();

            return Ok(new
            {
                msg = "Update Brand Successfully"
            });

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var brandInDB = await _brandRepository.GetOneAsync(e => e.Id == id);
            if (brandInDB is null)
                return NotFound();

            _brandRepository.Delete(brandInDB);
            await _brandRepository.CommitAsync();

            return Ok(new
            {
                msg = "Delete Brand Successfully"
            });

        }

    }
}
