using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Book;
using BookStoreAPI.Models.DTOs.Dashboard;
using BookStoreAPI.Models.DTOs.Revenue;
using BookStoreAPI.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public DashboardController(BookStoreDBContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public ActionResult<DashboardStatsResponse> GetStats()
        {
            var response = new DashboardStatsResponse
            {
                TotalUsers = _context.Users.Count(u => u.Role == "User"),
                TotalBooks = _context.Books.Count(),
                TotalCombos = _context.Combos.Count(),
                TotalOrders = _context.Orders.Count(),
                TotalSales = (decimal)_context.Orders
    .Where(o => o.Status == "Đã giao")
    .Sum(o => o.TotalAmount)
            };

            return Ok(response);
        }
        [HttpGet("revenue/weekly")]
        public ActionResult<IEnumerable<RevenueResponse>> GetWeeklyRevenue()
        {
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;

            var revenue = _context.Orders
       .Where(o => o.OrderDate != null && o.Status == "Đã giao") // 👈 chỉ tính đã giao
       .AsEnumerable()
       .GroupBy(o => new
       {
           Year = o.OrderDate.Value.Year,
           Week = calendar.GetWeekOfYear(
               o.OrderDate.Value,
               System.Globalization.CalendarWeekRule.FirstDay,
               DayOfWeek.Monday
           )
       })
       .Select(g => new RevenueResponse
       {
           Label = $"Tuần {g.Key.Week}/{g.Key.Year}",
           TotalRevenue = g.Sum(o => o.TotalAmount ?? 0)
       })
       .OrderBy(r => r.Label)
       .ToList();


            return Ok(revenue);
        }
        [HttpGet("revenue/monthly")]
        public ActionResult<IEnumerable<RevenueResponse>> GetMonthlyRevenue()
        {
            var revenue = _context.Orders
     .Where(o => o.OrderDate.HasValue && o.Status == "Đã giao") // 👈
     .GroupBy(o => new
     {
         Year = o.OrderDate.Value.Year,
         Month = o.OrderDate.Value.Month
     })
     .Select(g => new
     {
         g.Key.Year,
         g.Key.Month,
         TotalRevenue = g.Sum(o => o.TotalAmount ?? 0)
     })
     .AsEnumerable()
     .Select(g => new RevenueResponse
     {
         Label = $"{g.Month:00}/{g.Year}",
         TotalRevenue = g.TotalRevenue
     })
     .OrderBy(r => r.Label)
     .ToList();


            return Ok(revenue);
        }

        [HttpGet("revenue/yearly")]
        public ActionResult<IEnumerable<RevenueResponse>> GetYearlyRevenue()
        {
            var revenue = _context.Orders
      .Where(o => o.OrderDate.HasValue && o.Status == "Đã giao") // 👈
      .GroupBy(o => o.OrderDate.Value.Year)
      .Select(g => new
      {
          Year = g.Key,
          TotalRevenue = g.Sum(o => o.TotalAmount ?? 0)
      })
      .AsEnumerable()
      .Select(g => new RevenueResponse
      {
          Label = g.Year.ToString(),
          TotalRevenue = g.TotalRevenue
      })
      .OrderBy(r => r.Label)
      .ToList();


            return Ok(revenue);
        }
        [HttpGet("best-sellers")]
        public async Task<ActionResult<ResultCustomModel<List<BookBestSellerResponse>>>> GetBestSellers()
        {
            var bestSellers = await _context.OrderItems
                .GroupBy(oi => oi.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .Join(_context.Books, g => g.BookId, b => b.BookId, (g, b) => new BookBestSellerResponse
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    TotalSold = g.TotalSold,
                    CoverImageUrl = string.IsNullOrEmpty(b.CoverImage) ? null : $"{Request.Scheme}://{Request.Host}/images/books/{b.CoverImage}"
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<BookBestSellerResponse>>
            {
                Success = true,
                Message = $"Top {bestSellers.Count} sách bán chạy nhất",
                Data = bestSellers
            });
        }

        // DTO mới
        public class BookBestSellerResponse
        {
            public int BookId { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public int TotalSold { get; set; } // 👈 đây là số lượng bán
            public string CoverImageUrl { get; set; }
        }



    }
}
