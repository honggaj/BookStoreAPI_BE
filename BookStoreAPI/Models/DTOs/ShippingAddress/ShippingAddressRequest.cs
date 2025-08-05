namespace BookStoreAPI.Models.DTOs.ShippingAddress
{
    public class ShippingAddressRequest
    {
        public int UserId { get; set; }
        public string RecipientName { get; set; }
        public string Address { get; set; }
      
        public string PhoneNumber { get; set; }
    }   
}
