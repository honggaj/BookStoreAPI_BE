using BookStoreAPI.Models;
using BookStoreAPI.Models.Response; // ✅ result model
using BookStoreAPI.Models.DTOs.Review;
using BookStoreAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public ReviewController(BookStoreDBContext context)
        {
            _context = context;
        }

        // GET: api/Review
        [HttpGet]
        public async Task<ActionResult<ResultCustomModel<List<ReviewResponse>>>> GetAll()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .Select(r => new ReviewResponse
                {
                    ReviewId = r.ReviewId,
                    BookId = r.BookId ?? 0,
                    BookTitle = r.Book.Title,
                    UserId = r.UserId ?? 0,
                    Username = r.User.Username,
                    Rating = r.Rating ?? 0,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<ReviewResponse>>
            {
                Success = true,
                Message = "Lấy tất cả đánh giá thành công",
                Data = reviews
            });
        }

        // GET: api/Review/Book/1
        [HttpGet("Book/{bookId}")]
        public async Task<ActionResult<ResultCustomModel<List<ReviewResponse>>>> GetByBook(int bookId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.BookId == bookId)
                .Include(r => r.User)
                .Include(r => r.Book)
                .Select(r => new ReviewResponse
                {
                    ReviewId = r.ReviewId,
                    BookId = r.BookId ?? 0,
                    BookTitle = r.Book.Title,
                    UserId = r.UserId ?? 0,
                    Username = r.User.Username,
                    Rating = r.Rating ?? 0,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<ReviewResponse>>
            {
                Success = true,
                Message = $"Lấy đánh giá sách Id {bookId} thành công",
                Data = reviews
            });
        }

        // POST: api/Review/Create
        [HttpPost("Create")]
        public async Task<ActionResult<ResultCustomModel<object>>> Create(ReviewRequest request)
        {
            var review = new Review
            {
                BookId = request.BookId,
                UserId = request.UserId,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã thêm đánh giá",
                Data = new { id = review.ReviewId }
            });
        }

        [HttpPut("Update/{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Update(int id, ReviewRequest request)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy đánh giá",
                    Data = null
                });
            }

            // Check UserId: chỉ cho phép user tạo review được update
            if (review.UserId != request.UserId)
            {
                return Forbid(); // hoặc dùng Unauthorized(...) nếu muốn
            }

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.ReviewDate = DateTime.Now;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã cập nhật đánh giá",
                Data = null
            });
        }


        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Delete(int id, [FromQuery] int userId)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy đánh giá để xóa",
                    Data = null
                });
            }

            // ✅ Kiểm tra quyền xóa
            if (review.UserId != userId)
            {
                return Forbid(); // 403
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã xóa đánh giá",
                Data = null
            });
        }

    }
}
