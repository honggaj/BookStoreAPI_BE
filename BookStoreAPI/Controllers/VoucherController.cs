using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Voucher;
using BookStoreAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public VoucherController(BookStoreDBContext context)
        {
            _context = context;
        }

        // GET: api/voucher
        [HttpGet]
        public async Task<ActionResult<ResultCustomModel<List<VoucherResponse>>>> GetAll()
        {
            var vouchers = await _context.Vouchers
                .Select(v => MapToResponse(v))
                .ToListAsync();

            return Ok(new ResultCustomModel<List<VoucherResponse>>
            {
                Success = true,
                Message = $"⭐ Lấy {vouchers.Count} voucher thành công",
                Data = vouchers
            });
        }

        // POST: api/voucher
        [HttpPost]
        public async Task<ActionResult<ResultCustomModel<string>>> Create(VoucherRequest request)
        {
            if (_context.Vouchers.Any(v => v.Code == request.Code))
                return BadRequest(new ResultCustomModel<string>
                {
                    Success = false,
                    Message = "❌ Mã voucher đã tồn tại",
                    Data = null
                });

            var voucher = new Voucher
            {
                Code = request.Code,
                DiscountPercent = request.DiscountPercent,
                MaxDiscount = request.MaxDiscount,
                ExpiryDate = request.ExpiryDate,
                MinOrderAmount = request.MinOrderAmount,
                UsageLimit = request.UsageLimit,
                UsedCount = 0
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<string>
            {
                Success = true,
                Message = "✅ Tạo voucher thành công",
                Data = request.Code
            });
        }

        // DELETE: api/voucher/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Delete(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "❌ Không tìm thấy voucher",
                    Data = null
                });

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "🗑️ Xóa voucher thành công",
                Data = null
            });
        }

        // helper: map entity => response
        private static VoucherResponse MapToResponse(Voucher v)
        {
            return new VoucherResponse
            {
                VoucherId = v.VoucherId,
                Code = v.Code,
                DiscountPercent = v.DiscountPercent,
                MaxDiscount = v.MaxDiscount,
                ExpiryDate = v.ExpiryDate,
                MinOrderAmount = v.MinOrderAmount,
                UsageLimit = v.UsageLimit,
                UsedCount = v.UsedCount
            };
        }
    }
}
