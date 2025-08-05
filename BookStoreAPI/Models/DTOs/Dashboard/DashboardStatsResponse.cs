namespace BookStoreAPI.Models.DTOs.Dashboard
{

    public class DashboardStatsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalCombos { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
    }
}
