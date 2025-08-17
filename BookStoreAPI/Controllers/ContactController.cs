using BookStoreAPI.Models.DTOs.Contact;
using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace BookStoreAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ContactController(IConfiguration config)
        {
            _config = config;
        }

        // POST: api/contact/send
        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] ContactFormDto contact)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"]);
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];
            var enableSsl = bool.Parse(_config["Smtp:EnableSsl"]);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("BookStore", smtpUser));
            email.To.Add(new MailboxAddress("Admin", smtpUser)); // Có thể đổi thành email admin riêng
            email.Subject = contact.Subject ?? "Liên hệ từ website";

            email.Body = new TextPart("plain")
            {
                Text = $"Tên: {contact.Name}\nEmail: {contact.Email}\nSĐT: {contact.Phone}\n\nNội dung:\n{contact.Message}"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser, smtpPass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return Ok(new { success = true, message = "📨 Gửi email thành công!" });
        }
    }
}
