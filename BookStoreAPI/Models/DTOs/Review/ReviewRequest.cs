namespace BookStoreAPI.Models.DTOs.Review
{
    public class ReviewRequest
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; } // Từ 1 đến 5
        public string Comment { get; set; }
    }
}
