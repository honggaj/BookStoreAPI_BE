using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.User;
using BookStoreAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<ResultCustomModel<List<UserResponse>>>> GetAll()
        {
            var users = await _context.Users
                .Select(u => new UserResponse
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<UserResponse>>
            {
                Success = true,
                Message = "Lấy danh sách người dùng thành công",
                Data = users
            });
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ResultCustomModel<UserResponse>>> GetById(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null)
            {
                return NotFound(new ResultCustomModel<UserResponse>
                {
                    Success = false,
                    Message = "Không tìm thấy người dùng",
                    Data = null
                });
            }

            return Ok(new ResultCustomModel<UserResponse>
            {
                Success = true,
                Message = "Lấy thông tin người dùng thành công",
                Data = new UserResponse
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                }
            });
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(UserRequest request)
        {
            // Nếu không có role thì gán mặc định
            var role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;

            // Hash password
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
                Message = "Đã thêm user",
                Data = new { id = user.UserId }
            });
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UserUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Username = request.Username;
            user.Email = request.Email;
            user.Role = request.Role;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã cập nhật user",
                Data = null
            });
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy user",
                    Data = null
                });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã xóa user",
                Data = null
            });
        }


        [HttpGet("Search")]
        public async Task<IActionResult> Search(string keyword)
        {
            var result = await _context.Users
                .Where(u => u.Username.Contains(keyword) || u.Email.Contains(keyword))
                .Select(u => new UserResponse
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<UserResponse>>
            {
                Success = true,
                Message = $"Đã tìm thấy {result.Count} user",
                Data = result
            });
        }
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound(new ResultCustomModel<object> { Success = false, Message = "Không tìm thấy user" });

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
                return BadRequest(new ResultCustomModel<object> { Success = false, Message = "Mật khẩu cũ không đúng" });

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object> { Success = true, Message = "Đổi mật khẩu thành công" });
        }


    }
}
