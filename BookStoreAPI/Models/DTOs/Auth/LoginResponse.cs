namespace BookStoreAPI.Models.DTOs.Auth
{
    public class LoginResponse
    {
        public int UserId { get; set; }

        public string Token { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public string Role { get; set; }
    }
}
