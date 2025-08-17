using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Auth;
using BookStoreAPI.Services;
using Google.Apis.Auth; // nhớ import
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BookStoreDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public AuthController(BookStoreDBContext context, IConfiguration configuration, JwtService jwtService)
        {
            _context = context;
            _configuration = configuration;
            _jwtService = jwtService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized("Sai tài khoản hoặc mật khẩu");

            var token = GenerateJwtToken(user);

            return Ok(new LoginResponse
            {
                UserId = user.UserId,
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            });
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
                return BadRequest("Email đã được sử dụng.");

            if (_context.Users.Any(u => u.Username == request.Username))
                return BadRequest("Username đã tồn tại.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = hashedPassword,
                Role = "User"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            try
            {
                var smtp = _configuration.GetSection("Smtp");

                var message = new MailMessage();
                message.From = new MailAddress(smtp["Username"]);
                message.To.Add(user.Email);
                message.Subject = "🎉 Chào mừng đến với Tatsumaki BookStore!";
                message.Body = $"Xin chào {user.Username},\n\n" +
                               $"Cảm ơn bạn đã đăng ký tài khoản tại Tatsumaki BookStore!\n" +
                               $"Hãy khám phá thế giới sách đầy màu sắc nhé 📚✨\n\n" +
                               $"Truy cập ngay: http://localhost:4200";

                using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]));
                client.Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]);
                client.EnableSsl = bool.Parse(smtp["EnableSsl"]);
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi gửi email: " + ex.Message);
            }

            return Ok("Đăng ký thành công! Email chào mừng đã được gửi.");
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null) return NotFound("Email không tồn tại.");

            var token = Guid.NewGuid().ToString();
            user.ResetToken = token;
            user.ResetTokenExpiration = DateTime.UtcNow.AddMinutes(15);
            _context.SaveChanges();

            var smtp = _configuration.GetSection("Smtp");
            var message = new MailMessage();
            message.From = new MailAddress(smtp["Username"]);
            message.To.Add(user.Email);
            message.Subject = "Đặt lại mật khẩu BookStore";
            message.Body = $"Mã đặt lại mật khẩu của bạn: {token}";

            using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]));
            client.Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]);
            client.EnableSsl = bool.Parse(smtp["EnableSsl"]);
            client.Send(message);

            return Ok("Đã gửi mã khôi phục qua email.");
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null || user.ResetToken != request.Token || user.ResetTokenExpiration < DateTime.UtcNow)
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiration = null;

            _context.SaveChanges();
            return Ok("Đổi mật khẩu thành công.");
        }

        // POST: api/auth/google-login
        [HttpPost("google-login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var idToken = request.IdToken;
            var validPayload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            var email = validPayload.Email;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Username = validPayload.Name,
                    Email = email,
                    Password = "GOOGLE_LOGIN",
                    Role = "User"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var token = _jwtService.GenerateToken(user.Email, user.Role);

            return Ok(new LoginResponse
            {
                UserId = user.UserId,
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
