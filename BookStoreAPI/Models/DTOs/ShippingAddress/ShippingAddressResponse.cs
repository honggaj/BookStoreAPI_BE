namespace BookStoreAPI.Models.DTOs.ShippingAddress
{
    public class ShippingAddressResponse
    {
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string RecipientName { get; set; }
        public string Address { get; set; }
     
        public string PhoneNumber { get; set; }
    }
}
