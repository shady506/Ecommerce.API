using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoriesController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAsync();

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id);

            if (category is null)
                return NotFound();

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryRequest categoryRequest)
        {
            
            await _categoryRepository.CreateAsync(categoryRequest.Adapt<Category>());
            await _categoryRepository.CommitAsync();

            return Ok(new 
            {
                msg = "Add Category Successfully"
            });

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id ,CategoryRequest categoryRequest)
        {
            var categoryInDB = await _categoryRepository.GetOneAsync(e=>e.Id==id);
            if (categoryInDB is null)
                return NotFound();

            categoryInDB.Name = categoryRequest.Name;
            categoryRequest.Description = categoryInDB.Description;
            categoryRequest.Status = categoryInDB.Status;
            await _categoryRepository.CommitAsync();

            return Ok(new
            {
                msg = "Update Category Successfully"
            });

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoryInDB = await _categoryRepository.GetOneAsync(e => e.Id == id);
            if (categoryInDB is null)
                return NotFound();

            _categoryRepository.Delete(categoryInDB);
            await _categoryRepository.CommitAsync();

            return Ok(new
            {
                msg = "Delete Category Successfully"
            });

        }

    }
}
