namespace BookStoreAPI.Models.DTOs.Voucher
{
    public class VoucherRequest
    {
        public string Code { get; set; }
        public int DiscountPercent { get; set; }
        public decimal MaxDiscount { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int UsageLimit { get; set; }
    }
}
