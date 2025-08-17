using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.User;
using BookStoreAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public UserController(BookStoreDBContext context)
        {
            _context = context;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<ResultCustomModel<List<UserResponse>>>> GetAll()
        {
            var users = await _context.Users
                .Select(u => MapToResponse(u))
                .ToListAsync();

            return Ok(new ResultCustomModel<List<UserResponse>>
            {
                Success = true,
                Message = $"⭐ Lấy {users.Count} user thành công",
                Data = users
            });
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultCustomModel<UserResponse>>> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new ResultCustomModel<UserResponse>
                {
                    Success = false,
                    Message = "❌ Không tìm thấy user",
                    Data = null
                });

            return Ok(new ResultCustomModel<UserResponse>
            {
                Success = true,
                Message = "⭐ Lấy thông tin user thành công",
                Data = MapToResponse(user)
            });
        }

        // POST: api/user
        [HttpPost]
        public async Task<IActionResult> Create(UserRequest request)
        {
            var role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Password = hashedPassword,
                Email = request.Email,
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "✅ Đã thêm user",
                Data = new { id = user.UserId }
            });
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = request.Role;

            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "✅ Đã cập nhật user",
                Data = null
            });
        }

        // DELETE: api/user/{id}
        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .Include(u => u.ShippingAddresses) // 👈 load shipping addresses
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "❌ Không tìm thấy user",
                    Data = null
                });

            // Xóa shipping addresses trước
            if (user.ShippingAddresses.Any())
            {
                _context.ShippingAddresses.RemoveRange(user.ShippingAddresses);
            }

            // Sau đó xóa user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "🗑️ Đã xóa user và shipping addresses liên quan",
                Data = null
            });
        }


        // GET: api/user/search?keyword=...
        [HttpGet("search")]
        public async Task<IActionResult> Search(string keyword)
        {
            var users = await _context.Users
                .Where(u => u.Username.Contains(keyword) || u.Email.Contains(keyword))
                .Select(u => MapToResponse(u))
                .ToListAsync();

            return Ok(new ResultCustomModel<List<UserResponse>>
            {
                Success = true,
                Message = $"⭐ Đã tìm thấy {users.Count} user",
                Data = users
            });
        }

        // POST: api/user/changepassword
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound(new ResultCustomModel<object> { Success = false, Message = "❌ Không tìm thấy user" });

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
                return BadRequest(new ResultCustomModel<object> { Success = false, Message = "❌ Mật khẩu cũ không đúng" });

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object> { Success = true, Message = "✅ Đổi mật khẩu thành công" });
        }

        // helper: map entity => response
        private static UserResponse MapToResponse(User u)
        {
            return new UserResponse
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            };
        }
    }
}
