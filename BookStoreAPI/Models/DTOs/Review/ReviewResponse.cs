namespace BookStoreAPI.Models.DTOs.Review
{
    public class ReviewResponse
    {
        public int ReviewId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? ReviewDate { get; set; }
    }
}
