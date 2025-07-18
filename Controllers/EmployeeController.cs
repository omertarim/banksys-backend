using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankSysAPI.Data;
using BankSysAPI.Models;
using BankSysAPI.Services;
using System.Security.Claims;

namespace BankSysAPI.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly JwtService _jwtService;

        public EmployeeController(BankingDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateEmployee([FromBody] Employee newEmployee)
        {
            if (string.IsNullOrWhiteSpace(newEmployee.Email))
                return BadRequest("E-posta adresi zorunludur.");

            var user = new User
            {
                Email = newEmployee.Email,
                Username = newEmployee.UserName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(newEmployee.Password),
                FullName = $"{newEmployee.Name} {newEmployee.Surname}",
                RoleId = Role.Employee,
                IsApproved = true,
                Status = "Accepted"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            newEmployee.UserId = user.Id;
            newEmployee.RoleId = (int)Role.Employee;

            _context.Employees.Add(newEmployee);
            _context.SaveChanges();

            return Ok("Yeni çalışan başarıyla oluşturuldu.");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Geçersiz e-posta veya şifre.");

            if ((Role)user.RoleId != Role.Employee)
                return Forbid("Bu giriş yalnızca çalışanlar içindir.");

            if (!user.IsApproved)
                return Unauthorized("Çalışan hesabınız henüz onaylanmadı.");

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }
    }
}
