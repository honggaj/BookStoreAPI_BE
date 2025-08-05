namespace BookStoreAPI.Models.DTOs.User
{
    // Models/DTOs/User/ChangePasswordRequest.cs
    public class ChangePasswordRequest
    {
        public int UserId { get; set; } // hoặc lấy từ token cho chắc
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

}
