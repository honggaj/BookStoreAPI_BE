using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
                TotalSales = (decimal)_context.Orders.Sum(o => o.TotalAmount)
            };

            return Ok(response);
        }
    }
}
