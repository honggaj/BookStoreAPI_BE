namespace BookStoreAPI.Models.DTOs.Auth
{
    public class CustomerRegisterRequest
    {
        public string FullName { get; set; }


        public string Email { get; set; }

        public string Password { get; set; }
    }

}
