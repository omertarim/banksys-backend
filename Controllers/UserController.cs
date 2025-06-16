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
        public IActionResult Register(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == login.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = _jwtService.GenerateToken(user.Email);
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

            // GUID token oluştur ve sona erme süresi ata
            string resetToken = Guid.NewGuid().ToString();
            user.ResetToken = resetToken;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1); // 1 saat geçerli

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
