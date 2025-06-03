using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs;
using BookStoreAPI.Models.DTOs.Author.BookStoreAPI.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public AuthorsController(BookStoreDBContext context)
        {
            _context = context;
        }

        // GET: api/Authors
        [HttpGet("AuthorList")]
        public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAuthors()
        {
            var authors = await _context.Authors
                .Select(a => new AuthorResponse
                {
                    Id = a.Id,
                    Name = a.Name,
                    Nationality = a.Nationality
                }).ToListAsync();

            return Ok(authors);
        }

        // GET: api/Authors/5
        [HttpGet("AuthorById/{id}")]
        public async Task<ActionResult<AuthorResponse>> GetAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author == null)
                return NotFound();

            var response = new AuthorResponse
            {
                Id = author.Id,
                Name = author.Name,
                Nationality = author.Nationality
            };

            return Ok(response);
        }

        // POST: api/Authors
        [HttpPost("AuthorCreate")]
        public async Task<ActionResult<AuthorResponse>> CreateAuthor(AuthorRequest request)
        {
            var author = new Author
            {
                Name = request.Name,
                Nationality = request.Nationality
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            var response = new AuthorResponse
            {
                Id = author.Id,
                Name = author.Name,
                Nationality = author.Nationality
            };

            return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, response);
        }

        // PUT: api/Authors/5
        [HttpPut("AuthorUpdate/{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, AuthorRequest request)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author == null)
                return NotFound();

            author.Name = request.Name;
            author.Nationality = request.Nationality;

            _context.Entry(author).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Authors/5
        [HttpDelete("AuthorDelete/{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
                return NotFound();

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
