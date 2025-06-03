using BookStoreAPI.Models.DTOs.OrderDetail;

namespace BookStoreAPI.Models.DTOs.Order
{
    public class OrderRequest
    {
        public int CustomerId { get; set; } // Quan trọng nhất
        public string? Address { get; set; } // Optional — nếu KH chọn địa chỉ khác
        public string? City { get; set; }    // Optional
        public string? Country { get; set; } // Optional
        public string? Phone { get; set; }   // Optional
        public string Status { get; set; }
        public DateTime? OrderDate { get; set; } // <-- dòng bị thiếu nè

        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderDetailRequest> Items { get; set; }
    }

}
