namespace BookStoreAPI.Models.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Username { get; set; } // ⬅️ dòng đang thiếu
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
