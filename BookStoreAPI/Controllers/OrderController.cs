using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Customer;
using BookStoreAPI.Models.DTOs.Order;
using BookStoreAPI.Models.DTOs.OrderDetail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
        {
            return await _context.Orders
                .Select(o => new OrderResponse
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    OrderDate = o.OrderDate,
                    Address = o.Address,
                    City = o.City,
                    Country = o.Country,
                    Phone = o.Phone,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponse>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book) // lấy luôn sách
                .Include(o => o.Customer) // lấy info khách luôn
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return new OrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Address = order.Address,
                City = order.City,
                Country = order.Country,
                Phone = order.Phone,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                Items = order.OrderDetails.Select(od => new OrderDetailResponse
                {
                    BookId = od.BookId ?? 0,
                    BookTitle = od.Book?.Title ?? "",  // lấy tên sách nếu có
                    Quantity = od.Quantity ?? 0,
                    UnitPrice = od.UnitPrice ?? 0,
                    Discount = od.Discount ?? 0
                }).ToList(),
                Customer = order.Customer == null ? null : new CustomerResponse
                {
                    Id = order.Customer.Id,
                    FullName = order.Customer.FullName,
                    Email = order.Customer.Email,
                    Phone = order.Customer.Phone,
                    City = order.Customer.City,
                    Country = order.Customer.Country,
                    District = order.Customer.District,
                    Ward = order.Customer.Ward,
                    SpecificAddress = order.Customer.SpecificAddress,
                    Address = order.Customer.Address,
                    CreatedAt = order.Customer.CreatedAt
                }
            };
        }


        [HttpPost]
        public async Task<ActionResult<OrderResponse>> CreateOrder(OrderRequest request)
        {
            var customer = await _context.Customers.FindAsync(request.CustomerId);
            if (customer == null)
                return BadRequest("Customer không tồn tại");

            // Auto-fill từ customer nếu không có trong request
            var order = new Order
            {
                CustomerId = customer.Id,
                OrderDate = request.OrderDate ?? DateTime.UtcNow,
                Address = string.IsNullOrWhiteSpace(request.Address) ? customer.Address : request.Address,
                City = string.IsNullOrWhiteSpace(request.City) ? customer.City : request.City,
                Country = string.IsNullOrWhiteSpace(request.Country) ? customer.Country : request.Country,
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? customer.Phone : request.Phone,
                TotalAmount = request.TotalAmount,
                Status = request.Status,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = request.PaymentStatus
            };

            // Thêm OrderDetails
            foreach (var item in request.Items)
            {
                // Có thể validate book tồn tại nếu muốn chắc chắn
                var book = await _context.Books.FindAsync(item.BookId);
                if (book == null)
                    return BadRequest($"Book ID {item.BookId} không tồn tại.");

                order.OrderDetails.Add(new OrderDetail
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Trả về đơn hàng đã tạo (tối giản, chưa include BookTitle vì chưa load lại)
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new OrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Address = order.Address,
                City = order.City,
                Country = order.Country,
                Phone = order.Phone,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                Items = order.OrderDetails.Select(od => new OrderDetailResponse
                {
                    BookId = od.BookId ?? 0,
                    BookTitle = "", // chưa load Book nên để rỗng
                    Quantity = od.Quantity ?? 0,
                    UnitPrice = od.UnitPrice ?? 0,
                    Discount = od.Discount ?? 0
                }).ToList(),
                Customer = new CustomerResponse
                {
                    Id = customer.Id,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.Phone
                }
            });
        }



    }
}
