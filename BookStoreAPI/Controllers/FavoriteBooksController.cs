using Azure.Core;
using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Book;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
namespace BookStoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteBooksController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public FavoriteBooksController(BookStoreDBContext context)
        {
            _context = context;
        }

        public class AddFavoriteDto
        {
            public int CustomerId { get; set; }
            public int BookId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody] AddFavoriteDto input)
        {
            if (input.CustomerId <= 0 || input.BookId <= 0)
                return BadRequest("Invalid input.");

            var exists = await _context.FavoriteBooks
                .AnyAsync(f => f.CustomerId == input.CustomerId && f.BookId == input.BookId);

            if (exists)
                return BadRequest("Book already in favorites.");

            var favorite = new FavoriteBook
            {
                CustomerId = input.CustomerId,
                BookId = input.BookId,
                CreatedAt = DateTime.UtcNow
            };

            _context.FavoriteBooks.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok("Added to favorites.");
        }


        // DELETE: api/FavoriteBooks?customerId=1&bookId=2
        [HttpDelete]
        public async Task<IActionResult> RemoveFromFavorites(int customerId, int bookId)
        {
            var favorite = await _context.FavoriteBooks
                .FirstOrDefaultAsync(f => f.CustomerId == customerId && f.BookId == bookId);

            if (favorite == null)
                return NotFound("Favorite not found.");

            _context.FavoriteBooks.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok("Removed from favorites.");
        }

        // GET: api/FavoriteBooks/1
        [HttpGet("{customerId}")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetFavoriteBooks(int customerId)
        {
            var books = await _context.FavoriteBooks
                .Where(f => f.CustomerId == customerId)
                .Include(f => f.Book)
                .Select(f => f.Book)
                .ToListAsync();

            // Map từ Book sang BookResponse
            var bookResponses = books.Select(book => new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Price = book.Price,
                ImageUrl = book.Image,
                // map các trường khác cần thiết
            }).ToList();

            return Ok(bookResponses);
        }

    }

}
