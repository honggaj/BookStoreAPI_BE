namespace BookStoreAPI.Models.DTOs.Category
{
    public class CategoryRequest
    {
        public string Name { get; set; }
        public int? ParentId { get; set; }
    }
}
