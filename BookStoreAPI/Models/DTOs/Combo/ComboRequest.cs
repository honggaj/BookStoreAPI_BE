namespace BookStoreAPI.Models.DTOs.Combo
{
    public class ComboRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountPrice { get; set; }

        public IFormFile? Image { get; set; } // ✅ Thay vì string
        public List<int> BookIds { get; set; }
    }

}
