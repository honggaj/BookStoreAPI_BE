namespace BookStoreAPI.Models.DTOs.OrderItem
{
    public class OrderItemRequest
    {
        public int? BookId { get; set; }
        public int? ComboId { get; set; } // ✅ thêm dòng này
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}
