using BookStoreAPI.Models.DTOs;
using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public AdminController(BookStoreDBContext context)
        {
            _context = context;
        }
        // GET: api/Admin/AdminList
        [HttpGet("AdminList")]
        public async Task<ActionResult<IEnumerable<AdminResponse>>> GetAdminList()
        {
            var adminList = await _context.Admins
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AdminResponse
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Email = a.Email,
                    Role = a.Role,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(adminList);
        }
        // GET: api/Admin/AdminSearch?search=abc
        [HttpGet("AdminSearch")]
        public async Task<ActionResult<IEnumerable<AdminResponse>>> SearchAdmins([FromQuery] string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return BadRequest("Search keyword is required.");
            }

            string keyword = search.Trim().ToLower();

            var searchResults = await _context.Admins
                .Where(a =>
                    a.FullName.ToLower().Contains(keyword) ||
                    a.Email.ToLower().Contains(keyword) ||
                    a.Role.ToLower().Contains(keyword))
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AdminResponse
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Email = a.Email,
                    Role = a.Role,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(searchResults);
        }

        // POST: api/Admin
        [HttpPost("AdminCreate")]
        public async Task<ActionResult<AdminResponse>> CreateAdmin(AdminRequest request)
        {
            string hashedPassword = HashPassword(request.Password);

            var admin = new Admin
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = request.Role,
                CreatedAt = DateTime.Now
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdminById), new { id = admin.Id }, new AdminResponse
            {
                Id = admin.Id,
                FullName = admin.FullName,
                Email = admin.Email,
                Role = admin.Role,
                CreatedAt = admin.CreatedAt
            });
        }

        // GET: api/Admin/{id}
        [HttpGet("AdminById{id}")]
        public async Task<ActionResult<AdminResponse>> GetAdminById(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
                return NotFound();

            return new AdminResponse
            {
                Id = admin.Id,
                FullName = admin.FullName,
                Email = admin.Email,
                Role = admin.Role,
                CreatedAt = admin.CreatedAt
            };
        }

        // PUT: api/Admin/{id}
        [HttpPut("AdminUpdate{id}")]
        public async Task<IActionResult> UpdateAdmin(int id, AdminRequest request)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
                return NotFound();

            admin.FullName = request.FullName;
            admin.Email = request.Email;
            admin.Role = request.Role;

            if (!string.IsNullOrEmpty(request.Password))
            {
                admin.PasswordHash = HashPassword(request.Password);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Admin/{id}
        [HttpDelete("AdminDelete{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
                return NotFound();

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //private string HashPassword(string password)
        //{
        //    using var sha256 = SHA256.Create();
        //    var bytes = Encoding.UTF8.GetBytes(password);
        //    var hash = sha256.ComputeHash(bytes);
        //    return Convert.ToBase64String(hash);
        //}
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

    }
}
