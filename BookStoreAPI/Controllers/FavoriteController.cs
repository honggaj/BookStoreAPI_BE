using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Favorite;
using BookStoreAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public FavoriteController(BookStoreDBContext context)
        {
            _context = context;
        }

        // GET: api/favorite
        [HttpGet]
        public async Task<ActionResult<ResultCustomModel<List<FavoriteResponse>>>> GetAll()
        {
            var favs = await _context.Favorites
                .Include(f => f.User)
                .Include(f => f.Book)
                .Select(f => new FavoriteResponse
                {
                    FavoriteId = f.FavoriteId,
                    UserId = f.UserId ?? 0,
                    Username = f.User.Username,
                    BookId = f.BookId ?? 0,
                    BookTitle = f.Book.Title,
                    AddedDate = f.AddedDate,
                    CoverImageUrl = f.Book.CoverImage
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<FavoriteResponse>>
            {
                Success = true,
                Message = "📚 Lấy danh sách yêu thích thành công",
                Data = favs
            });
        }

        // GET: api/favorite/user/1
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ResultCustomModel<List<FavoriteResponse>>>> GetByUser(int userId)
        {
            var favs = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                .Include(f => f.User)
                .Select(f => new FavoriteResponse
                {
                    FavoriteId = f.FavoriteId,
                    UserId = f.UserId ?? 0,
                    Username = f.User.Username,
                    BookId = f.BookId ?? 0,
                    BookTitle = f.Book.Title,
                    AddedDate = f.AddedDate,
                    CoverImageUrl = f.Book.CoverImage
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<FavoriteResponse>>
            {
                Success = true,
                Message = "📌 Lấy danh sách yêu thích của người dùng thành công",
                Data = favs
            });
        }

        // POST: api/favorite
        [HttpPost]
        public async Task<ActionResult<ResultCustomModel<object>>> Add(FavoriteRequest request)
        {
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.BookId == request.BookId);

            if (existing != null)
                return BadRequest(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "⚠️ Sách này đã có trong yêu thích",
                    Data = null
                });

            var fav = new Favorite
            {
                UserId = request.UserId,
                BookId = request.BookId,
                AddedDate = DateTime.Now
            };

            _context.Favorites.Add(fav);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "✅ Đã thêm sách vào yêu thích",
                Data = new { id = fav.FavoriteId }
            });
        }

        // DELETE: api/favorite/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Delete(int id)
        {
            var fav = await _context.Favorites.FindAsync(id);
            if (fav == null)
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "❌ Không tìm thấy mục yêu thích",
                    Data = null
                });

            _context.Favorites.Remove(fav);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "🗑️ Đã xóa mục yêu thích",
                Data = null
            });
        }

        // DELETE: api/favorite/user-book?userId=1&bookId=2
        [HttpDelete("user-book")]
        public async Task<ActionResult<ResultCustomModel<object>>> DeleteByUserAndBook(int userId, int bookId)
        {
            var fav = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (fav == null)
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "❌ Không tìm thấy yêu thích để gỡ",
                    Data = null
                });

            _context.Favorites.Remove(fav);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "🗑️ Đã gỡ yêu thích khỏi sách",
                Data = null
            });
        }
    }
}
