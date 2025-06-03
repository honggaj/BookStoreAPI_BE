namespace BookStoreAPI.Models.DTOs.Admin
{
    public class AdminRequest
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; } // Plain password, to be hashed before saving

        public string Role { get; set; } = "Admin";
    }
}
