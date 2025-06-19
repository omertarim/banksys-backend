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
            bool isAdminEmail = request.Email.EndsWith("@admin.com");

            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsAdmin = isAdminEmail,
                IsApproved = !isAdminEmail // adminse onay bekleyecek
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Sadece onaylı kullanıcıya otomatik hesap oluştur
            if (user.IsApproved)
            {
                var iban = "TR" + Guid.NewGuid().ToString("N")[..24].ToUpper();

                var account = new Account
                {
                    UserId = user.Id,
                    IBAN = iban,
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow,
                    AccountType = "Cari",
                    Currency = "TL",
                    AccountNumber = $"{user.Id:0000}-001"
                };

                _context.Accounts.Add(account);
                _context.SaveChanges();
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Username,
                user.IsAdmin,
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



        [Authorize]
        [HttpPost("approve/{userId}")]
        public async Task<IActionResult> ApproveUser(int userId)
        {
            var requesterId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var requester = await _context.Users.FindAsync(requesterId);

            if (requester == null || !requester.IsAdmin)
                return Forbid("Bu işlemi yapma yetkiniz yok.");

            var userToApprove = await _context.Users.FindAsync(userId);

            if (userToApprove == null)
                return NotFound("Kullanıcı bulunamadı.");

            userToApprove.IsApproved = true;
            await _context.SaveChangesAsync();

            return Ok("Kullanıcı onaylandı.");
        }














    }
}
