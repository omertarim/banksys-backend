using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankSysAPI.Data;
using BankSysAPI.Models;
using BankSysAPI.Services; // JwtService için gerekli
using System.Security.Claims;

namespace BankSysAPI.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly JwtService _jwtService;

        public AdminController(BankingDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Geçersiz e-posta veya şifre.");

            if ((Role)user.RoleId != Role.Admin)
            return Unauthorized("Bu giriş yalnızca admin kullanıcılar içindir.");
            if (!user.IsApproved)
                return Unauthorized("Admin hesabınız henüz onaylanmadı.");

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAdminProfile()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var admin = _context.Admins.FirstOrDefault(a => a.UserId == userId);

            if (admin == null)
                return NotFound("Admin profili bulunamadı.");

            return Ok(admin);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllAdmins()
        {
            var admins = _context.Admins.ToList();
            return Ok(admins);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")] // İleride SuperAdmin'e özel yapılabilir
        public IActionResult CreateAdmin([FromBody] Admin newAdmin)
        {
            if (!newAdmin.Email.EndsWith("@admin.com"))
                return BadRequest("Sadece @admin.com uzantılı e-posta adresleri admin olabilir.");

            var user = new User
            {
                Email = newAdmin.Email,
                Username = newAdmin.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(newAdmin.Password),
                FullName = $"{newAdmin.Name} {newAdmin.Surname}",
                RoleId = Role.Admin,
                IsApproved = true,
                Status = "Accepted"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            newAdmin.UserId = user.Id;
            newAdmin.RoleId = (int)Role.Admin;

            _context.Admins.Add(newAdmin);
            _context.SaveChanges();

            return Ok("Yeni admin başarıyla oluşturuldu.");
        }
    }
}
