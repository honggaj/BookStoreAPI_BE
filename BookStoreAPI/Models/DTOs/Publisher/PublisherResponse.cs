using System.Collections.Generic;

namespace BookStoreAPI.Models.DTOs.Publisher
{
    public class PublisherResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public ICollection<BookStoreAPI.Models.Book> Books { get; set; } = new List<BookStoreAPI.Models.Book>();
    }
}
