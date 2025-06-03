namespace BookStoreAPI.Models.DTOs.Category
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }

        // Optionally, if you need to include more detailed information, you can include Book list, etc.
    }
}
