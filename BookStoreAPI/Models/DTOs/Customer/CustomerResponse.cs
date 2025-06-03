namespace BookStoreAPI.Models.DTOs.Customer
{
    public class CustomerResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string SpecificAddress { get; set; }

        public string Address { get; set; } // ✅ Thêm trường tổng hợp

        public DateTime? CreatedAt { get; set; }
    }
}
