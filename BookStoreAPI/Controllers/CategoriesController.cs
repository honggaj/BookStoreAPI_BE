using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs;
using BookStoreAPI.Models.DTOs.Category;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public CategoriesController(BookStoreDBContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet("CategoryList")]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Parent)
                .ToListAsync();

            var categoryResponses = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                ParentId = c.ParentId,
            }).ToList();

            return categoryResponses;
        }

        // GET: api/Categories/5
        [HttpGet("CategoryById{id}")]
        public async Task<ActionResult<CategoryResponse>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var categoryResponse = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId,
            };

            return categoryResponse;
        }

        // POST: api/Categories
        [HttpPost("CategoryCreate")]
        public async Task<ActionResult<CategoryResponse>> PostCategory(CategoryRequest categoryRequest)
        {
            var category = new Category
            {
                Name = categoryRequest.Name,
                ParentId = categoryRequest.ParentId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var categoryResponse = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId,
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryResponse);
        }

        // PUT: api/Categories/5
        [HttpPut("CategoryUpdate{id}")]
        public async Task<IActionResult> PutCategory(int id, CategoryRequest categoryRequest)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = categoryRequest.Name;
            category.ParentId = categoryRequest.ParentId;

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var categoryResponse = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId,
            };

            return Ok(categoryResponse);
        }

        // DELETE: api/Categories/5
        [HttpDelete("CategoryDelete{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
