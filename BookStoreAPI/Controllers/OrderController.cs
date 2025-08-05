using BookStoreAPI.Models;
using BookStoreAPI.Models.Response; // ✅ using cái này mới xài được ResultCustomModel<>
using BookStoreAPI.Models.DTOs.Order;
using BookStoreAPI.Models.DTOs.OrderItem;
using BookStoreAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreAPI.Models.DTOs.ShippingAddress; // Thêm dòng này
using Microsoft.AspNetCore.Authorization; // THÊM DÒNG NÀY Ở TRÊN


namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public OrderController(BookStoreDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ResultCustomModel<List<OrderResponse>>>> GetAll()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Book)
                .Include(o => o.ShippingAddress)
                .Select(o => new OrderResponse
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId ?? 0,
                    Username = o.User.Username,
                    ShippingAddressId = o.ShippingAddressId ?? 0,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    IsPaid = o.IsPaid,
                    Items = o.OrderItems.Select(oi => new OrderItemResponse
                    {
                        OrderItemId = oi.OrderItemId,

                        BookId = oi.BookId ?? 0,
                        BookTitle = oi.Book != null ? oi.Book.Title : null,

                        ComboId = oi.ComboId ?? 0,                              // ✅ thêm
                        ComboName = oi.Combo != null ? oi.Combo.Name : null, // ✅ thêm

                        Quantity = oi.Quantity,
                        Price = oi.Price
                    }).ToList(),

                    ShippingAddress = new ShippingAddressResponse
                    {
                        AddressId = o.ShippingAddress.AddressId,
                        UserId = o.ShippingAddress.UserId ?? 0,
                        Username = o.User.Username,
                        RecipientName = o.ShippingAddress.RecipientName,
                        Address = o.ShippingAddress.Address,
                        PhoneNumber = o.ShippingAddress.PhoneNumber
                    }
                }).ToListAsync();

            return Ok(new ResultCustomModel<List<OrderResponse>>
            {
                Success = true,
                Message = "Lấy tất cả đơn hàng thành công",
                Data = orders
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResultCustomModel<OrderResponse>>> GetById(int id)
        {
            var order = await _context.Orders
     .Include(o => o.User)
     .Include(o => o.OrderItems).ThenInclude(oi => oi.Book)
     .Include(o => o.OrderItems).ThenInclude(oi => oi.Combo) // ✅ thêm dòng này
     .Include(o => o.ShippingAddress)
     .Where(o => o.OrderId == id)
     .Select(o => new OrderResponse
     {
         OrderId = o.OrderId,
         UserId = o.UserId ?? 0,
         Username = o.User.Username,
         ShippingAddressId = o.ShippingAddressId ?? 0,
         OrderDate = o.OrderDate,
         TotalAmount = o.TotalAmount,
         Status = o.Status,
         PaymentMethod = o.PaymentMethod,
         IsPaid = o.IsPaid,
         Items = o.OrderItems.Select(oi => new OrderItemResponse
         {
             OrderItemId = oi.OrderItemId,
             BookId = oi.BookId ?? 0,
             BookTitle = oi.Book != null ? oi.Book.Title : null,
             ComboId = oi.ComboId ?? 0,                      // ✅ thêm dòng này
             ComboName = oi.Combo != null ? oi.Combo.Name : null, // ✅ thêm dòng này
             Quantity = oi.Quantity,
             Price = oi.Price
         }).ToList(),
         ShippingAddress = new ShippingAddressResponse
         {
             AddressId = o.ShippingAddress.AddressId,
             UserId = o.ShippingAddress.UserId ?? 0,
             Username = o.User.Username,
             RecipientName = o.ShippingAddress.RecipientName,
             Address = o.ShippingAddress.Address,
             PhoneNumber = o.ShippingAddress.PhoneNumber
         }
     }).FirstOrDefaultAsync();


            if (order == null)
            {
                return NotFound(new ResultCustomModel<OrderResponse>
                {
                    Success = false,
                    Message = "Không tìm thấy đơn hàng",
                    Data = null
                });
            }

            return Ok(new ResultCustomModel<OrderResponse>
            {
                Success = true,
                Message = "Lấy đơn hàng thành công",
                Data = order
            });
        }

        [HttpGet("User/{userId}")]
        public async Task<ActionResult<ResultCustomModel<List<OrderResponse>>>> GetByUser(int userId)
        {
            var orders = await _context.Orders
     .Where(o => o.UserId == userId)
     .Include(o => o.User)
     .Include(o => o.OrderItems).ThenInclude(oi => oi.Book)
     .Include(o => o.OrderItems).ThenInclude(oi => oi.Combo) // ✅ thêm dòng này
     .Include(o => o.ShippingAddress)
     .Select(o => new OrderResponse
     {
         OrderId = o.OrderId,
         UserId = o.UserId ?? 0,
         Username = o.User.Username,
         ShippingAddressId = o.ShippingAddressId ?? 0,
         OrderDate = o.OrderDate,
         TotalAmount = o.TotalAmount,
         Status = o.Status,
         PaymentMethod = o.PaymentMethod,
         IsPaid = o.IsPaid,
         Items = o.OrderItems.Select(oi => new OrderItemResponse
         {
             OrderItemId = oi.OrderItemId,
             BookId = oi.BookId ?? 0,
             BookTitle = oi.Book != null ? oi.Book.Title : null,
             ComboId = oi.ComboId ?? 0,                      // ✅ thêm dòng này
             ComboName = oi.Combo != null ? oi.Combo.Name : null, // ✅ thêm dòng này
             Quantity = oi.Quantity,
             Price = oi.Price
         }).ToList(),
         ShippingAddress = new ShippingAddressResponse
         {
             AddressId = o.ShippingAddress.AddressId,
             UserId = o.ShippingAddress.UserId ?? 0,
             Username = o.User.Username,
             RecipientName = o.ShippingAddress.RecipientName,
             Address = o.ShippingAddress.Address,
             PhoneNumber = o.ShippingAddress.PhoneNumber
         }
     }).ToListAsync();


            return Ok(new ResultCustomModel<List<OrderResponse>>
            {
                Success = true,
                Message = "Lấy đơn hàng theo người dùng thành công",
                Data = orders
            });
        }

        [HttpPost("Create")]
        public async Task<ActionResult<ResultCustomModel<object>>> Create(OrderRequest request)
        {
            int? addressId = request.ShippingAddressId;

            if (addressId == null)
            {
                var newAddress = new ShippingAddress
                {
                    UserId = request.UserId,
                    RecipientName = request.RecipientName,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber
                };

                _context.ShippingAddresses.Add(newAddress);
                await _context.SaveChangesAsync();
                addressId = newAddress.AddressId;
            }

            decimal total = request.Items.Sum(i => i.Price * i.Quantity);

            // ✅ Voucher xử lý
            Voucher voucher = null;
            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                voucher = await _context.Vouchers.FirstOrDefaultAsync(v =>
                    v.Code == request.VoucherCode &&
                    v.ExpiryDate >= DateOnly.FromDateTime(DateTime.Today) &&
                    v.UsageLimit > v.UsedCount &&
                    total >= v.MinOrderAmount);

                if (voucher == null)
                {
                    return BadRequest(new ResultCustomModel<object>
                    {
                        Success = false,
                        Message = "Voucher không hợp lệ hoặc đã hết hạn.",
                        Data = null
                    });
                }

                var discount = Math.Min(total * voucher.DiscountPercent.Value / 100, voucher.MaxDiscount ?? 0);
                total -= discount;

                // Trừ lượt sử dụng
                voucher.UsedCount = (voucher.UsedCount ?? 0) + 1;
            }

            var orderItems = request.Items.Select(i => new OrderItem
            {
                BookId = i.BookId,
                ComboId = i.ComboId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            var order = new Order
            {
                UserId = request.UserId,
                ShippingAddressId = addressId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                Status = "Chờ xử lý",
                PaymentMethod = request.PaymentMethod,
                IsPaid = request.IsPaid,
                VoucherId = voucher?.VoucherId,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);

            // ✅ TRỪ SỐ LƯỢNG KHO
            foreach (var item in orderItems)
            {
                if (item.BookId != null)
                {
                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book == null || book.Stock < item.Quantity)
                    {
                        return BadRequest(new ResultCustomModel<object>
                        {
                            Success = false,
                            Message = $"Sách ID {item.BookId} không đủ hàng.",
                            Data = null
                        });
                    }

                    book.Stock -= item.Quantity;
                }

                if (item.ComboId != null)
                {
                    var comboBooks = await _context.ComboBooks
                        .Where(cb => cb.ComboId == item.ComboId)
                        .Include(cb => cb.Book) // để lấy được book.Stock
                        .ToListAsync();

                    foreach (var cb in comboBooks)
                    {
                        var book = cb.Book;
                        if (book == null)
                        {
                            return BadRequest(new ResultCustomModel<object>
                            {
                                Success = false,
                                Message = $"Combo chứa sách không hợp lệ.",
                                Data = null
                            });
                        }

                        int totalQtyNeeded = item.Quantity; // 1 combo = 1 sách mỗi loại
                        if (book.Stock < totalQtyNeeded)
                        {
                            return BadRequest(new ResultCustomModel<object>
                            {
                                Success = false,
                                Message = $"Sách '{book.Title}' trong combo ID {item.ComboId} không đủ hàng.",
                                Data = null
                            });
                        }

                        book.Stock -= totalQtyNeeded;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đặt hàng thành công",
                Data = new { id = order.OrderId }
            });
        }



        // POST: api/Order/Create
        //[HttpPost("Create")]
        //public async Task<ActionResult<ResultCustomModel<object>>> Create(OrderRequest request)
        //{
        //    var total = request.Items.Sum(i => i.Price * i.Quantity);

        //    var order = new Order
        //    {
        //        UserId = request.UserId,
        //        ShippingAddressId = request.ShippingAddressId,
        //        OrderDate = DateTime.Now,
        //        TotalAmount = total,
        //        Status = "Chờ xử lý",
        //        OrderItems = request.Items.Select(i => new OrderItem
        //        {
        //            BookId = i.BookId,
        //            Quantity = i.Quantity,
        //            Price = i.Price
        //        }).ToList()
        //    };

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    return Ok(new ResultCustomModel<object>
        //    {
        //        Success = true,
        //        Message = "Đặt hàng thành công",
        //        Data = new { id = order.OrderId }
        //    });
        //}

        // PUT: api/Order/UpdateStatus/5?status=Đã giao
        [HttpPut("UpdateStatus/{id}")]
        public async Task<ActionResult<ResultCustomModel<string>>> UpdateStatus(int id, [FromQuery] string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new ResultCustomModel<string>
                {
                    Success = false,
                    Message = "Không tìm thấy đơn hàng",
                    Data = null
                });
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<string>
            {
                Success = true,
                Message = "Đã cập nhật trạng thái đơn hàng",
                Data = status
            });
        }

        // DELETE: api/Order/Delete/5
        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy đơn hàng",
                    Data = null
                });
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã xóa đơn hàng",
                Data = null
            });
        }
    }
}
