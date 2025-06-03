using Microsoft.AspNetCore.Mvc;
using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs;
using BCrypt.Net;
using BookStoreAPI.Models.DTOs.Auth;
using BookStoreAPI.Services;

namespace BookStoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BookStoreDBContext _context;
        private readonly JwtService _jwtService;

        public AuthController(BookStoreDBContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Đăng nhập vào hệ thống
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <returns>Thông tin người dùng và token</returns>
        [HttpPost("Login")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Email == request.Email);
            if (admin != null && BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
            {
                var token = _jwtService.GenerateToken(admin.Email, "admin");
                var response = new LoginResponse
                {
                    Token = token,
                    FullName = admin.FullName,
                    Email = admin.Email,
                    Id = admin.Id   // thêm ID vào response
                };
                return Ok(response);
            }

            var customer = _context.Customers.FirstOrDefault(c => c.Email == request.Email);
            if (customer != null && BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
            {
                var token = _jwtService.GenerateToken(customer.Email, "customer");
                var response = new LoginResponse
                {
                    Token = token,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Id = customer.Id   // thêm ID vào response
                };
                return Ok(response);
            }

            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
        }

    }
}