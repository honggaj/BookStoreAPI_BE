using BookStoreAPI.Models;
using BookStoreAPI.Models.Response; // ✅ model chuẩn
using BookStoreAPI.Models.DTOs.ShippingAddress;
using BookStoreAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingAddressController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public ShippingAddressController(BookStoreDBContext context)
        {
            _context = context;
        }

        // GET: api/ShippingAddress
        [HttpGet]
        public async Task<ActionResult<ResultCustomModel<List<ShippingAddressResponse>>>> GetAll()
        {
            var addresses = await _context.ShippingAddresses
                .Include(a => a.User)
                .Select(a => new ShippingAddressResponse
                {
                    AddressId = a.AddressId,
                    UserId = a.UserId ?? 0,
                    Username = a.User.Username,
                    RecipientName = a.RecipientName,
                    Address = a.Address,
                    
                    PhoneNumber = a.PhoneNumber
                }).ToListAsync();

            return Ok(new ResultCustomModel<List<ShippingAddressResponse>>
            {
                Success = true,
                Message = "Lấy danh sách địa chỉ thành công",
                Data = addresses
            });
        }

        // GET: api/ShippingAddress/User/1
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<ResultCustomModel<List<ShippingAddressResponse>>>> GetByUser(int userId)
        {
            var addresses = await _context.ShippingAddresses
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .Select(a => new ShippingAddressResponse
                {
                    AddressId = a.AddressId,
                    UserId = a.UserId ?? 0,
                    Username = a.User.Username,
                    RecipientName = a.RecipientName,
                    Address = a.Address,
                 
                    PhoneNumber = a.PhoneNumber
                }).ToListAsync();

            return Ok(new ResultCustomModel<List<ShippingAddressResponse>>
            {
                Success = true,
                Message = $"Lấy địa chỉ người dùng Id {userId} thành công",
                Data = addresses
            });
        }

        // POST: api/ShippingAddress/Create
        [HttpPost("Create")]
        public async Task<ActionResult<ResultCustomModel<object>>> Create(ShippingAddressRequest request)
        {
            var address = new ShippingAddress
            {
                UserId = request.UserId,
                RecipientName = request.RecipientName,
                Address = request.Address,
               
                PhoneNumber = request.PhoneNumber
            };

            _context.ShippingAddresses.Add(address);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã thêm địa chỉ mới",
                Data = new { id = address.AddressId }
            });
        }

        // PUT: api/ShippingAddress/Update/5
        [HttpPut("Update/{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Update(int id, ShippingAddressRequest request)
        {
            var address = await _context.ShippingAddresses.FindAsync(id);
            if (address == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy địa chỉ",
                    Data = null
                });
            }

            address.RecipientName = request.RecipientName;
            address.Address = request.Address;
        
            address.PhoneNumber = request.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã cập nhật địa chỉ",
                Data = null
            });
        }

        // DELETE: api/ShippingAddress/Delete/5
        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Delete(int id)
        {
            var address = await _context.ShippingAddresses.FindAsync(id);
            if (address == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy địa chỉ để xóa",
                    Data = null
                });
            }

            _context.ShippingAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã xóa địa chỉ",
                Data = null
            });
        }
    }
}
