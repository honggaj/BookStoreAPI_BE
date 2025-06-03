using Azure.Core;
using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs;
using BookStoreAPI.Models.DTOs.Customer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public CustomerController(BookStoreDBContext context)
        {
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] CustomerRequest request)
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Vui lòng nhập đầy đủ họ tên, email và mật khẩu.");
            }

            if (await _context.Customers.AnyAsync(c => c.Email == request.Email))
                return BadRequest("Email đã tồn tại.");

            var hashedPassword = HashPassword(request.Password);
            var address = $"{request.SpecificAddress}, {request.Ward}, {request.District}, {request.City}, {request.Country}".Replace(",,", ",").TrimEnd(',');

            var customer = new Customer
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone ?? "",
                District = request.District ?? "",
                Ward = request.Ward ?? "",
                SpecificAddress = request.SpecificAddress ?? "",
                City = request.City ?? "",
                Country = request.Country ?? "",
                Address = address, // ✅ Gán trường tổng hợp
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công" });
        }

        // GET: api/Customer/CustomerById5
        [HttpGet("CustomerById{id}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound();

            var response = new CustomerResponse
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                District = customer.District,
                Ward = customer.Ward,
                SpecificAddress = customer.SpecificAddress,
                City = customer.City,
                Country = customer.Country,
                Address = customer.Address, // ✅ thêm dòng này
                CreatedAt = customer.CreatedAt
            };



            return Ok(response);
        }

        // ✅ GET: api/Customer/List
        [HttpGet("CustomerList")]
        public async Task<ActionResult<IEnumerable<CustomerResponse>>> CustomerList()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerResponse
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    Email = c.Email,
                    Phone = c.Phone,
                    District = c.District,
                    Ward = c.Ward,
                    SpecificAddress = c.SpecificAddress,
                    City = c.City,
                    Country = c.Country,
                    Address=c.Address,
                    CreatedAt = c.CreatedAt
                }).ToListAsync();

            return Ok(customers);
        }

        [HttpPut("CustomerUpdate/{id}")]
        public async Task<IActionResult> CustomerUpdate(int id, [FromBody] CustomerUpdateRequest request)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound("Không tìm thấy khách hàng.");

            // ✅ Cập nhật các trường khác, KHÔNG thay đổi Password
            customer.FullName = request.FullName;
            customer.Email = request.Email;
            customer.Phone = request.Phone ?? "";
            customer.District = request.District ?? "";
            customer.Ward = request.Ward ?? "";
            customer.SpecificAddress = request.SpecificAddress ?? "";
            customer.City = request.City ?? "";
            customer.Country = request.Country ?? "";

            customer.Address = $"{customer.SpecificAddress}, {customer.Ward}, {customer.District}, {customer.City}, {customer.Country}"
                                .Replace(",,", ",").TrimEnd(',');

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thông tin thành công." });
        }


        // ✅ GET: api/Customer/Search?keyword=John
        [HttpGet("CustomerSearch")]
        public async Task<ActionResult<IEnumerable<CustomerResponse>>> CustomerSearch([FromQuery] string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return BadRequest("Từ khóa tìm kiếm không hợp lệ.");

            var customers = await _context.Customers
                .Where(c =>
                    c.FullName.Contains(keyword) ||
                    c.Email.Contains(keyword) ||
                    c.Phone.Contains(keyword))
                .Select(c => new CustomerResponse
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    Email = c.Email,
                    Phone = c.Phone,
                    District = c.District,
                    Ward = c.Ward,
                  SpecificAddress = c.SpecificAddress,
                    City = c.City,
                    Country = c.Country,
                    CreatedAt = c.CreatedAt
                }).ToListAsync();

            return Ok(customers);
        }

        // ✅ DELETE: api/Customer/Delete/5
        [HttpDelete("CustomerDelete/{id}")]
        public async Task<IActionResult> CustomerDelete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound("Không tìm thấy khách hàng.");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa khách hàng thành công." });
        }

        // 🔐 Helper: Hash password (use stronger algorithm in production)
        //private string HashPassword(string password)
        //{
        //    using var sha = SHA256.Create();
        //    var bytes = Encoding.UTF8.GetBytes(password);
        //    var hash = sha.ComputeHash(bytes);
        //    return Convert.ToBase64String(hash);
        //}
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
