namespace BookStoreAPI.Models.DTOs.Revenue
{
    public class RevenueResponse
    {
        public string Label { get; set; } // Ví dụ: "Tuần 32/2025", "08/2025", "2025"
        public decimal TotalRevenue { get; set; }
    }
}
