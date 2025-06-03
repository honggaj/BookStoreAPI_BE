using BookStoreAPI.Models;
using BookStoreAPI.Models.DTOs.Publisher;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublisherController : ControllerBase
    {
        private readonly BookStoreDBContext _context;

        public PublisherController(BookStoreDBContext context)
        {
            _context = context;
        }

        // GET: api/publisher
        [HttpGet("PublisherList")]
        public async Task<ActionResult<IEnumerable<PublisherResponse>>> GetPublishers()
        {
            var publishers = await _context.Publishers
                .Select(p => new PublisherResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Phone = p.Phone,
                    Email = p.Email,
                    Books = p.Books.ToList() // you might want to limit the fields of books
                }).ToListAsync();

            return Ok(publishers);
        }

        // GET: api/publisher/5
        [HttpGet("PublisherById{id}")]
        public async Task<ActionResult<PublisherResponse>> GetPublisher(int id)
        {
            var publisher = await _context.Publishers
                .Where(p => p.Id == id)
                .Select(p => new PublisherResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Phone = p.Phone,
                    Email = p.Email,
                    Books = p.Books.ToList()
                })
                .FirstOrDefaultAsync();

            if (publisher == null)
            {
                return NotFound();
            }

            return Ok(publisher);
        }

        // POST: api/publisher
        [HttpPost("PublisherCreate")]
        public async Task<ActionResult<PublisherResponse>> PostPublisher(PublisherRequest publisherRequest)
        {
            // Tạo một đối tượng Publisher mới mà không cần phải chỉ định Id
            var publisher = new Publisher
            {
                Name = publisherRequest.Name,
                Address = publisherRequest.Address,
                Phone = publisherRequest.Phone,
                Email = publisherRequest.Email
            };

            // Thêm đối tượng Publisher vào DbContext
            _context.Publishers.Add(publisher);

            // Lưu vào cơ sở dữ liệu và tự động gán Id cho Publisher
            await _context.SaveChangesAsync();

            // Tạo PublisherResponse để trả lại thông tin nhà xuất bản
            var publisherResponse = new PublisherResponse
            {
                Id = publisher.Id,  // Id đã được tự động sinh ra khi SaveChangesAsync
                Name = publisher.Name,
                Address = publisher.Address,
                Phone = publisher.Phone,
                Email = publisher.Email,
                Books = publisher.Books.ToList()  // Bạn có thể lọc lại sách nếu cần
            };

            // Trả về mã 201 Created với thông tin của nhà xuất bản
            return CreatedAtAction("GetPublisher", new { id = publisher.Id }, publisherResponse);
        }

        // PUT: api/publisher/5
        [HttpPut("PublisherUpdate{id}")]
        public async Task<IActionResult> PutPublisher(int id, PublisherRequest publisherRequest)
        {
            // Kiểm tra xem id từ URL có hợp lệ không
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound(); // Nếu không tìm thấy publisher theo id
            }

            // Cập nhật các thuộc tính của Publisher
            publisher.Name = publisherRequest.Name;
            publisher.Address = publisherRequest.Address;
            publisher.Phone = publisherRequest.Phone;
            publisher.Email = publisherRequest.Email;

            // Đánh dấu Publisher là đã thay đổi
            _context.Entry(publisher).State = EntityState.Modified;

            // Lưu các thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về phản hồi NoContent sau khi cập nhật thành công
            return NoContent();
        }


        // DELETE: api/publisher/5
        [HttpDelete("PublisherDelete{id}")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
