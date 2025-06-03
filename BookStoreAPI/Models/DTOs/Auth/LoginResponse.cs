namespace BookStoreAPI.Models.DTOs.Auth
{
    public class LoginResponse
    {
        public int Id { get; set; }

        public string Token { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }

}
