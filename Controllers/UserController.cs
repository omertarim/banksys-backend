using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankSysAPI.Data;
using BankSysAPI.Models;
using BankSysAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BankSysAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;

        public UserController(BankingDbContext context, JwtService jwtService, EmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            var email = request.Email?.Trim().ToLower();

            var user = new User
            {
                Email = email,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                //FullName = request.FullName,
                IsApproved = false,
                RoleId = email != null && email.EndsWith("@admin.com") ? Role.Admin : Role.Customer,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Eğer Customer ise Customer tablosuna da ekle
            if (user.RoleId == Role.Customer)
            {
                var customer = new Customer
                {
                    UserId = user.Id,
                    Status = "Pending",
                    CreateDate = DateTime.UtcNow,
                    LastUpdateDate = DateTime.UtcNow,
                    CustomerNumber = $"C{user.Id:D6}", // örnek müşteri numarası üretimi
                    HostIp = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                _context.Customers.Add(customer);
                _context.SaveChanges();
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Username,
                //user.FullName,
                user.RoleId,
                user.IsApproved
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Geçersiz e-posta veya şifre.");

            if (!user.IsApproved)
                return Unauthorized("Hesabınız henüz onaylanmadı. Lütfen bir yöneticiyle iletişime geçin.");

            var token = _jwtService.GenerateToken(user);

            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("secret")]
        public IActionResult Secret() => Ok("This endpoint is secured by token");

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            string resetToken = Guid.NewGuid().ToString();
            user.ResetToken = resetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            _context.SaveChanges();

            string resetLink = $"http://localhost:5173/reset-password?token={resetToken}";

            _emailService.SendPasswordResetEmail(user.Email, resetLink);

            return Ok("Password reset link has been sent to your email.");
        }

        [HttpGet("reset-password")]
        public IActionResult ValidateResetLink([FromQuery] string token)
        {
            var user = _context.Users.SingleOrDefault(u => u.ResetToken == token && u.ResetTokenExpires > DateTime.UtcNow);
            if (user == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            return Ok("Token is valid. You can now reset your password.");
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = _context.Users.SingleOrDefault(u => u.ResetToken == request.Token && u.ResetTokenExpires > DateTime.UtcNow);
            if (user == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            _context.SaveChanges();

            return Ok("Password has been successfully reset.");
        }

        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            user.IsApproved = true;
            user.Status = "Accepted";
            user.UpdatedAt = DateTime.UtcNow;

            // İlgili müşteri kaydını bul ve status'ü senkronize et
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == id);
            if (customer != null)
            {
                customer.Status = "Accepted";
                customer.LastUpdateDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok("Kullanıcı ve müşteri başarıyla onaylandı.");
        }


        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            user.IsApproved = false;
            user.Status = "Rejected";
            user.UpdatedAt = DateTime.UtcNow;


            await _context.SaveChangesAsync();
            // İlgili müşteri kaydını bul ve status'ü senkronize et
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == id);
            if (customer != null)
            {
                customer.Status = "Rejected";
                customer.LastUpdateDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok("Kullanıcı ve müşteri başarıyla reddedildi.");
        }








       




        [HttpGet("pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingUsers()
        {
            var requesterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var requester = await _context.Users.FindAsync(requesterId);
            if (requester == null || requester.RoleId != Role.Admin)
                return Forbid("Bu işlemi yapma yetkiniz yok.");

            var users = await _context.Users
                .Where(u => u.Status == "Pending")
                .Select(u => new {
                    u.Id,
                    Name = u.Username, // <--- Use Username as Name
                    u.Email
                })
                .ToListAsync();

            return Ok(users);
        }

        [Authorize]
        [HttpGet("all")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.Select(u => new
            {
                u.Id,
                Name = u.Username, // or u.FullName if you want
                u.Email,
                u.IsApproved,
                u.RoleId
            }).ToList();

            return Ok(users);
        }
    }
}
