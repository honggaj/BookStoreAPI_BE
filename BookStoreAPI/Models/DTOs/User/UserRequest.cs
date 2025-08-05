namespace BookStoreAPI.Models.DTOs.User
{
    public class UserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; } // Nếu bảo mật thì hash nhé
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
