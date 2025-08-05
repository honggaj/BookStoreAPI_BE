namespace BookStoreAPI.Models.DTOs.Favorite
{
    public class FavoriteResponse
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string CoverImageUrl { get; set; } // 👈 Thêm dòng này
        public DateTime? AddedDate { get; set; }
    }
}
