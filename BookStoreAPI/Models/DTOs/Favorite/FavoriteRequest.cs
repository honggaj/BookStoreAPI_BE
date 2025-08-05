namespace BookStoreAPI.Models.DTOs.Favorite
{
    public class FavoriteRequest
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
    }
}
