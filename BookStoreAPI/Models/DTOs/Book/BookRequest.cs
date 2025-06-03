namespace BookStoreAPI.Models.DTOs.Book
{
    public class BookRequest
    {
        public string Title { get; set; }

        public string Isbn { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int? Stock { get; set; }

        public DateOnly? PublishDate { get; set; }

        public IFormFile ImageFile { get; set; }

        public int? CategoryId { get; set; }

        public int? PublisherId { get; set; }

        public List<int> AuthorIds { get; set; }  // Danh sách tác giả (Id)
    }
}
