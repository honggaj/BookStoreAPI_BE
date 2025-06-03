using BookStoreAPI.Models.DTOs.Customer;
using BookStoreAPI.Models.DTOs.OrderDetail;

namespace BookStoreAPI.Models.DTOs.Order
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        public CustomerResponse Customer { get; set; } // 💥 thêm chỗ này
        public List<OrderDetailResponse> Items { get; set; }
    }
}
