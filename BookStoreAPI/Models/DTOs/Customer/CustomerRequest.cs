using System.ComponentModel.DataAnnotations;

namespace BookStoreAPI.Models.DTOs.Customer
{
    public class CustomerRequest
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Phone { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string SpecificAddress { get; set; }
        public string Address { get; set; } // ✅ Thêm trường tổng hợp
    }
}
