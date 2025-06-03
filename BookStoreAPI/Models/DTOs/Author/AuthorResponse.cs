namespace BookStoreAPI.Models.DTOs.Author
{
    namespace BookStoreAPI.Models.Dtos
    {
        public class AuthorResponse
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Nationality { get; set; }

            // Có thể mở rộng thêm sách nếu muốn
            // public List<string> BookTitles { get; set; }
        }
    }

}
