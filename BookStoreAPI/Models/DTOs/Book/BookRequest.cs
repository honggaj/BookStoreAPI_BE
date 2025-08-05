namespace BookStoreAPI.Models.DTOs.Book
{
    public class BookRequest
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int? GenreId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public DateOnly PublishedDate { get; set; }
        public IFormFile? CoverImage { get; set; } // Ảnh nè
    }
}
