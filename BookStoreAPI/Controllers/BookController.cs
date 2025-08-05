    using BookStoreAPI.Models;
    using BookStoreAPI.Models.DTOs.Book;
    using BookStoreAPI.Models.Response;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    namespace BookStoreAPI.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class BookController : ControllerBase
        {
            private readonly BookStoreDBContext _context;
            private readonly IWebHostEnvironment _env;

            public BookController(BookStoreDBContext context, IWebHostEnvironment env)
            {
                _context = context;
                _env = env;
            }
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("API sống!");
        }

        [HttpGet]
            public async Task<ActionResult<ResultCustomModel<List<BookResponse>>>> GetAll()
            {
                var books = await _context.Books
                    .Select(b => new BookResponse
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        Author = b.Author,
                        GenreId = b.GenreId,
                        Price = b.Price,
                        Stock = b.Stock,
                        Description = b.Description,
                        PublishedDate = b.PublishedDate,
                        CoverImageUrl = string.IsNullOrEmpty(b.CoverImage)
                            ? null
                            : $"{Request.Scheme}://{Request.Host}/images/books/{b.CoverImage}"
                    })
                    .ToListAsync(); // ✅ Tách ra khỏi EF Core để xử lý tiếp

                // Tính average rating bằng LINQ thuần sau khi lấy xong
                var ratingDict = await _context.Reviews
                    .Where(r => r.Rating.HasValue)
                    .GroupBy(r => r.BookId)
                    .Select(g => new
                    {
                        BookId = g.Key,
                        AvgRating = g.Average(r => r.Rating.Value)
                    })
                    .ToDictionaryAsync(x => x.BookId ?? 0, x => (decimal?)x.AvgRating);

                // Gán average rating vào từng book
                foreach (var book in books)
                {
                    if (ratingDict.TryGetValue(book.BookId, out var avg))
                    {
                        book.AverageRating = avg;
                    }
                    else
                    {
                        book.AverageRating = null;
                    }
                }

                return Ok(new ResultCustomModel<List<BookResponse>>
                {
                    Success = true,
                    Message = $"Tìm thấy {books.Count} sách",
                    Data = books
                });
            }


            [HttpGet("{id}")]
            public async Task<ActionResult<ResultCustomModel<BookResponse>>> GetById(int id)
            {
                var b = await _context.Books.FindAsync(id);
                if (b == null)
                    return NotFound(new ResultCustomModel<object> { Success = false, Message = "Không tìm thấy sách" });

                // ✅ Tính average rating cho sách này
                var avgRating = await _context.Reviews
                    .Where(r => r.BookId == id && r.Rating.HasValue)
                    .AverageAsync(r => (double?)r.Rating) ?? 0;

                var result = new BookResponse
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    GenreId = b.GenreId,
                    Price = b.Price,
                    Stock = b.Stock,
                    Description = b.Description,
                    PublishedDate = b.PublishedDate,
                    CoverImageUrl = string.IsNullOrEmpty(b.CoverImage)
                        ? null
                        : $"{Request.Scheme}://{Request.Host}/images/books/{b.CoverImage}",
                    AverageRating = Math.Round((decimal)avgRating, 1) // 👈 Gán vào nè
                };

                return Ok(new ResultCustomModel<BookResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin sách thành công",
                    Data = result
                });
            }


            // POST: api/Book/Create
            [HttpPost("Create")]
            public async Task<ActionResult<ResultCustomModel<object>>> Create([FromForm] BookRequest request)
            {
                var fileName = await SaveImageAsync(request.CoverImage);

                var book = new Book
                {
                    Title = request.Title,
                    Author = request.Author,
                    GenreId = request.GenreId,
                    Price = request.Price,
                    Stock = request.Stock,
                    Description = request.Description,
                    PublishedDate = request.PublishedDate,
                    CoverImage = fileName
                };

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                return Ok(new ResultCustomModel<object>
                {
                    Success = true,
                    Message = "Đã thêm sách",
                    Data = new { id = book.BookId }
                });
            }

            // PUT: api/Book/Update/5
            [HttpPut("Update/{id}")]
            public async Task<ActionResult<ResultCustomModel<object>>> Update(int id, [FromForm] BookRequest request)
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                    return NotFound(new ResultCustomModel<object> { Success = false, Message = "Không tìm thấy sách" });

                if (request.CoverImage != null)
                {
                    DeleteImage(book.CoverImage);
                    book.CoverImage = await SaveImageAsync(request.CoverImage);
                }

                book.Title = request.Title;
                book.Author = request.Author;
                book.GenreId = request.GenreId;
                book.Price = request.Price;
                book.Stock = request.Stock;
                book.Description = request.Description;
                book.PublishedDate = request.PublishedDate;

                await _context.SaveChangesAsync();

                return Ok(new ResultCustomModel<object>
                {
                    Success = true,
                    Message = "Đã cập nhật sách"
                });
            }

            // DELETE: api/Book/Delete/5
            [HttpDelete("Delete/{id}")]
            public async Task<ActionResult<ResultCustomModel<object>>> Delete(int id)
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                    return NotFound(new ResultCustomModel<object> { Success = false, Message = "Không tìm thấy sách" });

                DeleteImage(book.CoverImage);
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return Ok(new ResultCustomModel<object>
                {
                    Success = true,
                    Message = "Đã xóa sách"
                });
            }

            // GET: api/Book/Search?keyword=abc
            [HttpGet("Search")]
            public async Task<ActionResult<ResultCustomModel<List<BookResponse>>>> Search(string keyword)
            {
                var result = await _context.Books
                    .Where(b => b.Title.Contains(keyword) || b.Author.Contains(keyword))
                    .Select(b => new BookResponse
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        Author = b.Author,
                        GenreId = b.GenreId,
                        Price = b.Price,
                        Stock = b.Stock,
                        Description = b.Description,
                        PublishedDate = b.PublishedDate,
                        CoverImageUrl = string.IsNullOrEmpty(b.CoverImage)
                            ? null
                            : $"{Request.Scheme}://{Request.Host}/images/books/{b.CoverImage}"
                    })
                    .ToListAsync();

                return Ok(new ResultCustomModel<List<BookResponse>>
                {
                    Success = true,
                    Message = $"Tìm thấy {result.Count} sách",
                    Data = result
                });
            }
            [HttpGet("AdvancedSearch")]
            public async Task<ActionResult<ResultCustomModel<List<BookResponse>>>> AdvancedSearch(
        string? keyword,
        int? genreId,
        decimal? minPrice,
        decimal? maxPrice,
        DateOnly? publishedAfter,
        string? sortBy = "title", // title | price | date
        bool ascending = true)
            {
                var query = _context.Books.AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                    query = query.Where(b => b.Title.Contains(keyword) || b.Author.Contains(keyword));

                if (genreId.HasValue)
                    query = query.Where(b => b.GenreId == genreId);

                if (minPrice.HasValue)
                    query = query.Where(b => b.Price >= minPrice);

                if (maxPrice.HasValue)
                    query = query.Where(b => b.Price <= maxPrice);

                if (publishedAfter.HasValue)
                    query = query.Where(b => b.PublishedDate >= publishedAfter);

                // Sắp xếp
                query = sortBy switch
                {
                    "price" => ascending ? query.OrderBy(b => b.Price) : query.OrderByDescending(b => b.Price),
                    "date" => ascending ? query.OrderBy(b => b.PublishedDate) : query.OrderByDescending(b => b.PublishedDate),
                    _ => ascending ? query.OrderBy(b => b.Title) : query.OrderByDescending(b => b.Title),
                };

                var result = await query.Select(b => new BookResponse
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    GenreId = b.GenreId,
                    Price = b.Price,
                    Stock = b.Stock,
                    Description = b.Description,
                    PublishedDate = b.PublishedDate,
                    CoverImageUrl = string.IsNullOrEmpty(b.CoverImage)
                        ? null
                        : $"{Request.Scheme}://{Request.Host}/images/books/{b.CoverImage}"
                }).ToListAsync();

                return Ok(new ResultCustomModel<List<BookResponse>>
                {
                    Success = true,
                    Message = $"Tìm thấy {result.Count} sách",
                    Data = result
                });
            }

            // GET: api/Book/ByGenre/2
            [HttpGet("ByGenre/{genreId}")]
            public async Task<ActionResult<ResultCustomModel<List<BookResponse>>>> GetByGenre(int genreId)
            {
                var books = await _context.Books
                    .Where(b => b.GenreId == genreId)
                    .Select(b => new BookResponse
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        Author = b.Author,
                        GenreId = b.GenreId,
                        Price = b.Price,
                        Stock = b.Stock,
                        Description = b.Description,
                        PublishedDate = b.PublishedDate,
                        CoverImageUrl = string.IsNullOrEmpty(b.CoverImage)
                            ? null
                            : $"{Request.Scheme}://{Request.Host}/images/books/{b.CoverImage}"
                    })
                    .ToListAsync();

                return Ok(new ResultCustomModel<List<BookResponse>>
                {
                    Success = true,
                    Message = $"Tìm thấy {books.Count} sách thuộc thể loại {genreId}",
                    Data = books
                });
            }

            [HttpGet("best-sellers")]
            public async Task<ActionResult<ResultCustomModel<List<BookResponse>>>> GetBestSellers()
            {
                var bestSellers = await _context.OrderItems
                    .GroupBy(o => o.BookId)
                    .Select(g => new
                    {
                        BookId = g.Key,
                        TotalSold = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.TotalSold)
                    .Take(10)
                    .Join(_context.Books, g => g.BookId, b => b.BookId, (g, b) => b)
                    .Select(b => new BookResponse
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        Author = b.Author,
                        GenreId = b.GenreId,
                        Price = b.Price,
                        Stock = b.Stock,
                        Description = b.Description,
                        PublishedDate = b.PublishedDate,
                        CoverImageUrl = string.IsNullOrEmpty(b.CoverImage)
                            ? null
                            : $"{Request.Scheme}://{Request.Host}/images/books/{b.CoverImage}"
                    })
                    .ToListAsync();

                return Ok(new ResultCustomModel<List<BookResponse>>
                {
                    Success = true,
                    Message = $"Top {bestSellers.Count} sách bán chạy nhất",
                    Data = bestSellers
                });
            }

            [HttpGet("latest")]
            public async Task<ActionResult<ResultCustomModel<List<BookResponse>>>> GetLatestBooks()
            {
                var books = await _context.Books
                    .OrderByDescending(b => b.PublishedDate)
                    .Take(10)
                    .Select(b => new BookResponse
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        Author = b.Author,
                        GenreId = b.GenreId,
                        Price = b.Price,
                        Stock = b.Stock,
                        Description = b.Description,
                        PublishedDate = b.PublishedDate,
                        CoverImageUrl = string.IsNullOrEmpty(b.CoverImage)
                            ? null
                            : $"{Request.Scheme}://{Request.Host}/images/books/{b.CoverImage}"
                    })
                    .ToListAsync();

                return Ok(new ResultCustomModel<List<BookResponse>>
                {
                    Success = true,
                    Message = $"Top {books.Count} sách mới nhất",
                    Data = books
                });
            }


        [HttpGet("top-rated")]
        public async Task<ActionResult<ResultCustomModel<List<BookResponse>>>> GetTopRatedBooks()
        {
            // 1. Tính điểm trung bình của từng sách
            var avgRatings = await _context.Reviews
                .Where(r => r.Rating.HasValue)
                .GroupBy(r => r.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    AvgRating = g.Average(r => r.Rating.Value)
                })
                .OrderByDescending(x => x.AvgRating)
                .Take(10) // Lấy top 10
                .ToListAsync();

            var topBookIds = avgRatings.Select(x => x.BookId).ToList();

            // 2. Lấy thông tin sách
            var books = await _context.Books
                .Where(b => topBookIds.Contains(b.BookId))
                .ToListAsync();

            // 3. Map ra response
            var responses = books.Select(b => new BookResponse
            {
                BookId = b.BookId,
                Title = b.Title,
                Author = b.Author,
                GenreId = b.GenreId,
                Price = b.Price,
                Stock = b.Stock,
                Description = b.Description,
                PublishedDate = b.PublishedDate,
                CoverImageUrl = string.IsNullOrEmpty(b.CoverImage)
                    ? null
                    : $"{Request.Scheme}://{Request.Host}/images/books/{b.CoverImage}",
                AverageRating = Math.Round((decimal?)avgRatings.FirstOrDefault(x => x.BookId == b.BookId)?.AvgRating ?? 0, 1)
            })
            .OrderByDescending(x => x.AverageRating) // Đảm bảo thứ tự
            .ToList();

            return Ok(new ResultCustomModel<List<BookResponse>>
            {
                Success = true,
                Message = $"Top {responses.Count} sách được đánh giá cao nhất",
                Data = responses
            });
        }


        // Helpers
        private async Task<string> SaveImageAsync(IFormFile file)
            {
                if (file == null) return null;

                var folder = Path.Combine(_env.WebRootPath, "images", "books");
                Directory.CreateDirectory(folder); // đảm bảo tồn tại

                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                return fileName;
            }

            private void DeleteImage(string fileName)
            {
                if (string.IsNullOrEmpty(fileName)) return;

                var path = Path.Combine(_env.WebRootPath, "images", "books", fileName);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
        }
    }
