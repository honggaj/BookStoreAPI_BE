using BookStoreAPI.Models;
using BookStoreAPI.Models.Response; // ✅ dùng ResultCustomModel<>
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

        // GET: api/Favorite
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
                    AddedDate = f.AddedDate
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<FavoriteResponse>>
            {
                Success = true,
                Message = "Lấy danh sách yêu thích thành công",
                Data = favs
            });
        }

        // GET: api/Favorite/User/1
        [HttpGet("User/{userId}")]
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
                    CoverImageUrl = f.Book.CoverImage, // ✅ Gán đúng tên property
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<FavoriteResponse>>
            {
                Success = true,
                Message = "Lấy danh sách yêu thích của người dùng thành công",
                Data = favs
            });
        }

        // POST: api/Favorite/Add
        [HttpPost("Add")]
        public async Task<ActionResult<ResultCustomModel<object>>> Add(FavoriteRequest request)
        {
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.BookId == request.BookId);

            if (existing != null)
            {
                return BadRequest(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Đã yêu thích sách này rồi!",
                    Data = null
                });
            }

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
                Message = "Đã thêm vào yêu thích",
                Data = new { id = fav.FavoriteId }
            });
        }

        // DELETE: api/Favorite/Delete/5
        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Delete(int id)
        {
            var fav = await _context.Favorites.FindAsync(id);
            if (fav == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy mục yêu thích",
                    Data = null
                });
            }

            _context.Favorites.Remove(fav);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã xóa yêu thích",
                Data = null
            });
        }

        // DELETE: api/Favorite/DeleteByUserBook?userId=1&bookId=2
        [HttpDelete("DeleteByUserBook")]
        public async Task<ActionResult<ResultCustomModel<object>>> DeleteByUserAndBook(int userId, int bookId)
        {
            var fav = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (fav == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy yêu thích để gỡ",
                    Data = null
                });
            }

            _context.Favorites.Remove(fav);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã gỡ yêu thích khỏi sách",
                Data = null
            });
        }
    }
}
