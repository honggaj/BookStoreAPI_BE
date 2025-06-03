namespace BookStoreAPI.Models.DTOs.Book
{
    public class BookResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Isbn { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? Stock { get; set; }
        public DateOnly? PublishDate { get; set; }
        public string ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public int? PublisherId { get; set; }

        public List<string> AuthorNames { get; set; } // 👈 Thêm dòng này
    }

}
