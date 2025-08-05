using BookStoreAPI.Models.DTOs.ShippingAddress; // nhớ using nha

namespace BookStoreAPI.Models.DTOs.Order
{
    public class OrderItemResponse
    {
        public int OrderItemId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }


        public int? ComboId { get; set; }          // ✅ thêm dòng này
        public string ComboName { get; set; }      // ✅ thêm dòng này


        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class OrderResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ShippingAddressId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public bool? IsPaid { get; set; }

        public List<OrderItemResponse> Items { get; set; }

        // 👇 THÊM NÈ
        public ShippingAddressResponse ShippingAddress { get; set; }
    }
}
