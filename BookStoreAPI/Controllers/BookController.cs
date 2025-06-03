using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Book;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly BookStoreDBContext _context;
        private readonly IWebHostEnvironment _env;

        public BookController(BookStoreDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("BookList")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetBooks()
        {
            var books = await _context.Books
                .Include(b => b.Authors) // 👈 Include tác giả
                .Select(book => new BookResponse
                {
                    Id = book.Id,
                    Title = book.Title,
                    Isbn = book.Isbn,
                    Description = book.Description,
                    Price = book.Price,
                    Stock = book.Stock,
                    PublishDate = book.PublishDate,
                    ImageUrl = $"/images/books/{book.Image}",
                    CategoryId = book.CategoryId,
                    PublisherId = book.PublisherId,
                    AuthorNames = book.Authors.Select(a => a.Name).ToList() // 👈 Lấy tên tác giả
                })
                .ToListAsync();

            return books;
        }

        [HttpPost("BookCreate")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<BookResponse>> CreateBook([FromForm] BookRequest request)
        {
            string imageName = null;

            // Save image if provided
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "books");
                Directory.CreateDirectory(uploadsFolder);

                string fileExt = Path.GetExtension(request.ImageFile.FileName);
                string rawName = Path.GetFileNameWithoutExtension(request.ImageFile.FileName);

                imageName = Slugify(rawName) + "-" + Guid.NewGuid().ToString("N").Substring(0, 6) + fileExt;
                string filePath = Path.Combine(uploadsFolder, imageName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await request.ImageFile.CopyToAsync(stream);
            }

            // Lấy danh sách tác giả từ AuthorIds
            var authors = await _context.Authors
                .Where(a => request.AuthorIds.Contains(a.Id))
                .ToListAsync();

            var book = new Book
            {
                Title = request.Title,
                Isbn = request.Isbn,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                PublishDate = request.PublishDate,
                Image = imageName,
                CategoryId = request.CategoryId,
                PublisherId = request.PublisherId,
                Authors = authors // Gán trực tiếp để EF tạo liên kết
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                Description = book.Description,
                Price = book.Price,
                Stock = book.Stock,
                PublishDate = book.PublishDate,
                ImageUrl = $"/images/books/{book.Image}",
                CategoryId = book.CategoryId,
                PublisherId = book.PublisherId,
                AuthorNames = authors.Select(a => a.Name).ToList() // 👈 Thêm nếu muốn trả về tên tác giả
            });

        }

        // GET: api/Book/BookById/5
        [HttpGet("BookById/{id}")]
        public async Task<ActionResult<BookResponse>> GetBookById(int id)
        {
            var book = await _context.Books
                .Include(b => b.Authors) // 👈 Bao gồm dữ liệu tác giả
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            return new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                Description = book.Description,
                Price = book.Price,
                Stock = book.Stock,
                PublishDate = book.PublishDate,
                ImageUrl = $"/images/books/{book.Image}",
                CategoryId = book.CategoryId,
                PublisherId = book.PublisherId,
                AuthorNames = book.Authors.Select(a => a.Name).ToList() // 👈 Trích danh sách tên tác giả
            };
        }


        [HttpPut("BookUpdate/{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookRequest request)
        {
            var book = await _context.Books
                .Include(b => b.Authors) // Bao gồm Authors để cập nhật liên kết
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            // Cập nhật hình ảnh nếu có file mới
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "books");
                Directory.CreateDirectory(uploadsFolder);

                string fileExt = Path.GetExtension(request.ImageFile.FileName);
                string rawName = Path.GetFileNameWithoutExtension(request.ImageFile.FileName);
                string imageName = Slugify(rawName) + "-" + Guid.NewGuid().ToString("N").Substring(0, 6) + fileExt;
                string filePath = Path.Combine(uploadsFolder, imageName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await request.ImageFile.CopyToAsync(stream);

                // Xóa ảnh cũ
                if (!string.IsNullOrEmpty(book.Image))
                {
                    string oldImagePath = Path.Combine(uploadsFolder, book.Image);
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                book.Image = imageName;
            }

            // Cập nhật các trường thông tin sách
            if (!string.IsNullOrWhiteSpace(request.Title))
                book.Title = request.Title.Trim();

            if (!string.IsNullOrWhiteSpace(request.Isbn))
                book.Isbn = request.Isbn.Trim();

            if (!string.IsNullOrWhiteSpace(request.Description))
                book.Description = request.Description.Trim();

            if (request.Price > 0)
                book.Price = request.Price;

            if (request.Stock >= 0)
                book.Stock = request.Stock;

            if (request.PublishDate != default)
                book.PublishDate = request.PublishDate;

            if (request.CategoryId > 0)
                book.CategoryId = request.CategoryId;

            if (request.PublisherId > 0)
                book.PublisherId = request.PublisherId;

            // Cập nhật danh sách tác giả nếu có AuthorIds
            if (request.AuthorIds != null && request.AuthorIds.Any())
            {
                // Lấy các tác giả theo request.AuthorIds
                var authors = await _context.Authors
                    .Where(a => request.AuthorIds.Contains(a.Id))
                    .ToListAsync();

                // Cập nhật liên kết nhiều-nhiều
                book.Authors.Clear();   // Xóa hết tác giả cũ liên kết với sách này
                foreach (var author in authors)
                {
                    book.Authors.Add(author);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }


        // DELETE: api/Book/BookDelete/{id}
        [HttpDelete("BookDelete/{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            // Xóa hình ảnh khỏi thư mục nếu có
            if (!string.IsNullOrEmpty(book.Image))
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "books");
                string imagePath = Path.Combine(uploadsFolder, book.Image);
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        private string Slugify(string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            string cleaned = sb.ToString().Normalize(NormalizationForm.FormC);
            cleaned = Regex.Replace(cleaned.ToLower(), @"[^a-z0-9\s-]", "");
            cleaned = Regex.Replace(cleaned, @"\s+", "-").Trim('-');

            return cleaned;
        }
        // GET: api/Book/BooksByCategory/3
        [HttpGet("BooksByCategory/{categoryId}")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetBooksByCategory(int categoryId)
        {
            var books = await _context.Books
                .Where(b => b.CategoryId == categoryId)
                .Include(b => b.Authors)
                .Select(book => new BookResponse
                {
                    Id = book.Id,
                    Title = book.Title,
                    Isbn = book.Isbn,
                    Description = book.Description,
                    Price = book.Price,
                    Stock = book.Stock,
                    PublishDate = book.PublishDate,
                    ImageUrl = $"/images/books/{book.Image}",
                    CategoryId = book.CategoryId,
                    PublisherId = book.PublisherId,
                    AuthorNames = book.Authors.Select(a => a.Name).ToList()
                })
                .ToListAsync();

            return books;
        }
        [HttpGet("BooksByParentCategory/{parentId}")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetBooksByParentCategory(int parentId)
        {
            // Lấy category có id là parentId
            var parentCategory = await _context.Categories.FindAsync(parentId);
            if (parentCategory == null)
                return NotFound("Category not found");

            // Lấy tất cả ID của category con
            var childCategoryIds = await _context.Categories
                .Where(c => c.ParentId == parentId)
                .Select(c => c.Id)
                .ToListAsync();

            // Tạo danh sách categoryId để tìm sách (bao gồm parentId và các con)
            var categoryIdsToSearch = new List<int> { parentId };
            categoryIdsToSearch.AddRange(childCategoryIds);

            // Truy vấn sách theo các category này
            var books = await _context.Books
                .Include(b => b.Authors)
                .Where(b => b.CategoryId.HasValue && categoryIdsToSearch.Contains(b.CategoryId.Value))
                .Select(book => new BookResponse
                {
                    Id = book.Id,
                    Title = book.Title,
                    Isbn = book.Isbn,
                    Description = book.Description,
                    Price = book.Price,
                    Stock = book.Stock,
                    PublishDate = book.PublishDate,
                    ImageUrl = $"/images/books/{book.Image}",
                    CategoryId = book.CategoryId,
                    PublisherId = book.PublisherId,
                    AuthorNames = book.Authors.Select(a => a.Name).ToList()
                })
                .ToListAsync();

            return books;
        }


    }
}
