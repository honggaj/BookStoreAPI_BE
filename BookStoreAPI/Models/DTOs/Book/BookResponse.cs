namespace BookStoreAPI.Models.DTOs.Book
{
    using System;

  
        public class BookResponse
        {
            public int BookId { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public int? GenreId { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Description { get; set; }
            public DateOnly? PublishedDate { get; set; }
        public string GenreName { get; set; } // <-- thêm dòng này

        public string CoverImageUrl { get; set; } // URL full path nè
        public decimal? AverageRating { get; set; } // ⭐️ Thêm dòng này

    }


}
