namespace BookStoreAPI.Models.DTOs.OrderDetail
{
    public class OrderDetailResponse
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } // optional nếu bạn muốn hiện tên sách
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
    }
}
