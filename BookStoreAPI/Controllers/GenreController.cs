using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Genre;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreAPI.Models.Response;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public GenreController(BookStoreDBContext context)
        {
            _context = context;
        }

        // GET: api/Genre
        [HttpGet]
        public async Task<ActionResult<ResultCustomModel<List<GenreResponse>>>> GetAll()
        {
            var genres = await _context.Genres
                .Select(g => new GenreResponse
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<GenreResponse>>
            {
                Success = true,
                Message = $"Tìm thấy {genres.Count} thể loại",
                Data = genres
            });
        }


        // GET: api/Genre/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultCustomModel<GenreResponse>>> GetById(int id)
        {
            var g = await _context.Genres.FindAsync(id);
            if (g == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy thể loại",
                    Data = null
                });
            }

            var result = new GenreResponse
            {
                GenreId = g.GenreId,
                Name = g.Name
            };

            return Ok(new ResultCustomModel<GenreResponse>
            {
                Success = true,
                Message = "Lấy thông tin thể loại thành công",
                Data = result
            });
        }

        // POST: api/Genre/Create

        [HttpPost("Create")]
        public async Task<ActionResult<ResultCustomModel<object>>> Create(GenreRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Tên thể loại không được để trống",
                    Data = null
                });
            }

            var genre = new Genre { Name = request.Name };
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã thêm thể loại",
                Data = new { id = genre.GenreId }
            });
        }

        // PUT: api/Genre/Update/5
        [HttpPut("Update/{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Update(int id, GenreRequest request)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy thể loại",
                    Data = null
                });
            }

            genre.Name = request.Name;
            _context.Genres.Update(genre);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã cập nhật thể loại",
                Data = null
            });
        }

        // DELETE: api/Genre/Delete/5

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<ResultCustomModel<object>>> Delete(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound(new ResultCustomModel<object>
                {
                    Success = false,
                    Message = "Không tìm thấy thể loại",
                    Data = null
                });
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return Ok(new ResultCustomModel<object>
            {
                Success = true,
                Message = "Đã xóa thể loại",
                Data = null
            });
        }

        // GET: api/Genre/Search?keyword=abc

        [HttpGet("Search")]
        public async Task<ActionResult<ResultCustomModel<List<GenreResponse>>>> Search(string keyword)
        {
            var result = await _context.Genres
                .Where(g => g.Name.Contains(keyword))
                .Select(g => new GenreResponse
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                })
                .ToListAsync();

            return Ok(new ResultCustomModel<List<GenreResponse>>
            {
                Success = true,
                Message = $"Tìm thấy {result.Count} thể loại",
                Data = result
            });
        }
    }
}
