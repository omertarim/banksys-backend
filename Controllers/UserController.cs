using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankSysAPI.Data;
using BankSysAPI.Models;
using BankSysAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Yeni IBAN üret
            string iban = GenerateIban();

            // Aynı kullanıcı için daha önce açılmış hesap sayısı
            int existingCount = _context.Accounts.Count(a => a.UserId == user.Id);
            string accountNumber = $"{user.Id:D4}-{existingCount + 1:D2}";

            var account = new Account
            {
                UserId = user.Id,
                IBAN = iban,
                AccountNumber = accountNumber,
                AccountType = "Cari Hesap",
                Currency = "TL",
                Balance = 1000.00M,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            _context.SaveChanges();

            return Ok(new { user.Id, user.Email, user.Username });
        }

        private string GenerateIban()
        {
            var random = new Random();
            return "TR" + random.Next(100000000, 999999999) + random.Next(100000000, 999999999);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == login.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _jwtService.GenerateToken(user.Id, user.Email);
            return Ok(new { Token = token });
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

            string resetLink = $"http://localhost:5252/api/user/reset-password?token={resetToken}";
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
    }
}
