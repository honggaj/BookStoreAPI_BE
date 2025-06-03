namespace BookStoreAPI.Models.DTOs.OrderDetail
{
    public class OrderDetailRequest
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
    }
}
