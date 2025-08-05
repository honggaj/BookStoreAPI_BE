using BookStoreAPI.Models.DTOs.Book;

namespace BookStoreAPI.Models.DTOs.Combo
{
    public class ComboResponse
    {
        public int ComboId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public string? Image { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<BookResponse> Books { get; set; } = new();
    }
}
