namespace BookStoreAPI.Models.DTOs.User
{
    public class UserUpdateRequest
    {
        public string Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        // KHÔNG cần Password ở đây
    }
}
