namespace BookStoreAPI.Models.DTOs.OrderItem
{
    public class OrderRequest
    {
        public int? UserId { get; set; }
        public int? ShippingAddressId { get; set; }

        public string RecipientName { get; set; }
        public string Address { get; set; }
       
        public string PhoneNumber { get; set; }
        public string? VoucherCode { get; set; }  // ✅ rõ ràng: có thể null

        public string PaymentMethod { get; set; }  // <== thêm dòng này: "COD" hoặc "PayPal"
        public bool IsPaid { get; set; }           // <== đã thanh toán hay chưa

        public List<OrderItemRequest> Items { get; set; }
    }

}
