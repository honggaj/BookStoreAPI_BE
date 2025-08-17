using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Combo;
using BookStoreAPI.Models.DTOs.Book;
using BookStoreAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private readonly BookStoreDBContext _context;
        private readonly IWebHostEnvironment _env;

        public ComboController(BookStoreDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/combo
        [HttpGet]
        public async Task<ActionResult<ResultCustomModel<List<ComboResponse>>>> GetAll()
        {
            var combos = await _context.Combos
                .Include(c => c.ComboBooks)
                .ThenInclude(cb => cb.Book)
                .Select(c => new ComboResponse
                {
                    ComboId = c.ComboId,
                    Name = c.Name,
                    Description = c.Description,
                    TotalPrice = c.TotalPrice ?? 0,
                    DiscountPrice = c.DiscountPrice ?? 0,
                    Image = string.IsNullOrEmpty(c.Image)
                        ? null
                        : $"{Request.Scheme}://{Request.Host}/images/combos/{c.Image}",
                    CreatedDate = c.CreatedDate,
                    Books = c.ComboBooks.Select(cb => new BookResponse
                    {
                        BookId = cb.Book.BookId,
                        Title = cb.Book.Title,
                        Author = cb.Book.Author,
                        GenreId = cb.Book.GenreId,
                        Price = cb.Book.Price,
                        Stock = cb.Book.Stock,
                        Description = cb.Book.Description,
                        PublishedDate = cb.Book.PublishedDate,
                        CoverImageUrl = string.IsNullOrEmpty(cb.Book.CoverImage)
                            ? null
                            : $"{Request.Scheme}://{Request.Host}/images/books/{cb.Book.CoverImage}"
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<ComboResponse>>
            {
                Success = true,
                Message = $"Đã tìm thấy {combos.Count} combo",
                Data = combos
            });
        }

        // GET: api/combo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultCustomModel<ComboResponse>>> GetById(int id)
        {
            var combo = await _context.Combos
                .Include(c => c.ComboBooks)
                .ThenInclude(cb => cb.Book)
                .FirstOrDefaultAsync(c => c.ComboId == id);

            if (combo == null)
                return NotFound(new ResultCustomModel<ComboResponse>
                {
                    Success = false,
                    Message = "Combo không tồn tại"
                });

            var result = new ComboResponse
            {
                ComboId = combo.ComboId,
                Name = combo.Name,
                Description = combo.Description,
                TotalPrice = combo.TotalPrice ?? 0,
                DiscountPrice = combo.DiscountPrice ?? 0,
                Image = string.IsNullOrEmpty(combo.Image)
                    ? null
                    : $"{Request.Scheme}://{Request.Host}/images/combos/{combo.Image}",
                CreatedDate = combo.CreatedDate,
                Books = combo.ComboBooks.Select(cb => new BookResponse
                {
                    BookId = cb.Book.BookId,
                    Title = cb.Book.Title,
                    Author = cb.Book.Author,
                    GenreId = cb.Book.GenreId,
                    Price = cb.Book.Price,
                    Stock = cb.Book.Stock,
                    Description = cb.Book.Description,
                    PublishedDate = cb.Book.PublishedDate,
                    CoverImageUrl = string.IsNullOrEmpty(cb.Book.CoverImage)
                        ? null
                        : $"{Request.Scheme}://{Request.Host}/images/books/{cb.Book.CoverImage}"
                }).ToList()
            };

            return Ok(new ResultCustomModel<ComboResponse>
            {
                Success = true,
                Message = "🎯 Combo đã được tìm thấy",
                Data = result
            });
        }

        // POST: api/combo
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromForm] ComboRequest request)
        {
            string imageFileName = null;
            if (request.Image != null)
            {
                imageFileName = await SaveImageAsync(request.Image);
            }

            var combo = new Combo
            {
                Name = request.Name,
                Description = request.Description,
                TotalPrice = request.TotalPrice,
                DiscountPrice = request.DiscountPrice,
                Image = imageFileName,
                CreatedDate = DateTime.Now,
                ComboBooks = request.BookIds.Select(id => new ComboBook { BookId = id }).ToList()
            };

            _context.Combos.Add(combo);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "✅ Đã tạo combo" });
        }

        // PUT: api/combo/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> Update(int id, [FromForm] ComboRequest request)
        {
            var combo = await _context.Combos
                .Include(c => c.ComboBooks)
                .FirstOrDefaultAsync(c => c.ComboId == id);

            if (combo == null)
                return NotFound(new { success = false, message = "❌ Combo không tồn tại" });

            combo.Name = request.Name;
            combo.Description = request.Description;
            combo.TotalPrice = request.TotalPrice;
            combo.DiscountPrice = request.DiscountPrice;

            if (request.Image != null)
            {
                combo.Image = await SaveImageAsync(request.Image);
            }

            _context.ComboBooks.RemoveRange(combo.ComboBooks);
            combo.ComboBooks = request.BookIds.Select(id => new ComboBook { BookId = id }).ToList();

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "✅ Cập nhật combo thành công" });
        }

        // DELETE: api/combo/{id}
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var combo = await _context.Combos
                .Include(c => c.ComboBooks)
                .FirstOrDefaultAsync(c => c.ComboId == id);

            if (combo == null)
                return NotFound(new { success = false, message = "Combo không tồn tại" });

            _context.ComboBooks.RemoveRange(combo.ComboBooks);
            _context.Combos.Remove(combo);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "🗑️ Đã xoá combo" });
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null) return null;

            var folder = Path.Combine(_env.WebRootPath, "images", "combos");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }
    }
}
