using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/category
        [HttpGet]
        public IActionResult GetAll()
        {
            var categories = _categoryService.GetAll();
            return Ok(categories);
        }

        // GET: api/category/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var category = _categoryService.GetById(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            return Ok(category);
        }


        // POST: api/category
        [HttpPost]
        public IActionResult Create([FromBody] CategoryModel categoryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _categoryService.Create(categoryModel);
            return Ok(new { message = "Category created successfully." });
        }

        // PUT: api/category/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] CategoryModel categoryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingCategory = _categoryService.GetById(id);
            if (existingCategory == null)
                return NotFound(new { message = "Category not found." });

            _categoryService.Update(id, categoryModel);
            return Ok(new { message = "Category updated successfully." });
        }

        // DELETE: api/category/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _categoryService.Delete(id);
            return Ok(new { message = "Category deleted successfully." });
        }
    }
}