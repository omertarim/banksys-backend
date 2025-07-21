using Microsoft.AspNetCore.Mvc;
using BankSysAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankSysAPI.Data;
using System.ComponentModel.DataAnnotations;

namespace BankSysAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly IConfiguration _configuration;

        public CustomerController(BankingDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CustomerRegisterRequest request)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);

            var user = new User
            {
                Email = request.Email ?? "",
                Username = request.Name,
                FullName = request.Name, 
                PasswordHash = hashedPassword,
                RoleId = Role.Customer,
                IsActive = false,
                HostIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var lastCustomer = await _context.Customers
                .OrderByDescending(c => c.CustomerId)
                .FirstOrDefaultAsync();

            int newId = (lastCustomer?.CustomerId ?? 0) + 1;
            string newCustomerNumber = "CUST" + newId.ToString("D6");

            var customer = new Customer
            {
                User = user,
                UserId = user.Id,
                Email = user.Email,
                Name = user.Username,
                Status = user.Status, // Status eşitleniyor
                TaxNumber = request.TaxNumber,
                TaxOfficeId = request.TaxOfficeId,
                PersonTypeId = request.PersonTypeId,
                CitizenshipId = request.CitizenshipId,
                AccomodationId = request.AccomodationId,
                LanguageId = request.LanguageId,
                RecordingChannel = request.RecordingChannel,
                CitizenshipCountryId = request.CitizenshipCountryId,
                AccomodationCountryId = request.AccomodationCountryId,
                CustomerNumber = newCustomerNumber,
                CreateDate = DateTime.UtcNow,
                HostIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            await _context.CustomerEmails.AddAsync(new CustomerEmail
            {
                CustomerId = customer.CustomerId,
                Email = user.Email
            });

            await _context.SaveChangesAsync();

            return Ok($"Kayıt başarılı. Müşteri numaranız: {newCustomerNumber}");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CustomerLoginRequest request)
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .ToListAsync();

            var customer = customers.FirstOrDefault(c =>
                (c.CustomerNumber == request.CustomerNumber || c.CitizenshipCountryId == request.CitizenshipCountryId) &&
                BCrypt.Net.BCrypt.Verify(request.Password, c.User.PasswordHash)
            );

            if (customer == null)
                return Unauthorized("Bilgiler hatalı.");

            if (!customer.User.IsApproved)
                return Unauthorized("Hesabınız henüz onaylanmamış.");

            var claims = new[]
            {
                new Claim("CustomerNumber", customer.CustomerNumber),
                new Claim(ClaimTypes.NameIdentifier, customer.User.Id.ToString()),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCustomerInfo(int id, [FromBody] CustomerUpdateRequest request)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Emails)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
                return NotFound("Müşteri bulunamadı.");

            customer.Name = request.Name ?? customer.Name;
            customer.TaxNumber = request.TaxNumber ?? customer.TaxNumber;
            customer.TaxOfficeId = request.TaxOfficeId ?? customer.TaxOfficeId;
            customer.LanguageId = request.LanguageId ?? customer.LanguageId;
            customer.PersonTypeId = request.PersonTypeId ?? customer.PersonTypeId;
            customer.CitizenshipId = request.CitizenshipId ?? customer.CitizenshipId;
            customer.AccomodationId = request.AccomodationId ?? customer.AccomodationId;
            customer.RecordingChannel = request.RecordingChannel ?? customer.RecordingChannel;
            customer.CitizenshipCountryId = request.CitizenshipCountryId;
            customer.AccomodationCountryId = request.AccomodationCountryId;
            customer.LastUpdateDate = DateTime.UtcNow;

            if (customer.User != null)
            {
                customer.User.Username = request.Name ?? customer.User.Username;
                customer.User.UpdatedAt = DateTime.UtcNow;
            }

            var validEmails = request.Emails?.Where(email =>
            {
                var validator = new EmailAddressAttribute();
                return validator.IsValid(email) && !customer.Emails.Any(e => e.Email == email);
            }).ToList();

            if (validEmails != null && validEmails.Any())
            {
                foreach (var email in validEmails)
                {
                    customer.Emails.Add(new CustomerEmail
                    {
                        CustomerId = customer.CustomerId,
                        Email = email
                    });
                }
            }

            var allEmails = customer.Emails.Select(e => e.Email).ToList();
            if (allEmails.Count == 1)
            {
                customer.Email = allEmails[0];
                if (customer.User != null)
                {
                    customer.User.Email = allEmails[0];
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Müşteri bilgileri başarıyla güncellendi.");
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetCustomerByUserId(int userId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
                return NotFound("Müşteri bulunamadı.");

            return Ok(new { customer.CustomerId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Emails)
                .Include(c => c.PersonType)
                .Include(c => c.TaxOffice) // Uncomment after you add these navigation properties
                .Include(c => c.Citizenship)
                .Include(c => c.Accomodation)
                .Include(c => c.Language)
                // .Include(c => c.RecordingChannel)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
                return NotFound("Müşteri bulunamadı.");

            return Ok(new
            {
                customer.CustomerId,
                customer.Name,
                customer.TaxNumber,
                personTypeId = customer.PersonTypeId,
                personTypeName = customer.PersonType?.Name,
                taxOfficeId = customer.TaxOfficeId,
                taxOfficeName = customer.TaxOffice?.Name,
                citizenshipId = customer.CitizenshipId,
                citizenshipName = customer.Citizenship?.Name,
                accomodationId = customer.AccomodationId,
                accomodationName = customer.Accomodation?.Name,
                languageId = customer.LanguageId,
                languageName = customer.Language?.Name,
                // recordingChannelId = customer.RecordingChannelId,
                // recordingChannelName = customer.RecordingChannel?.Name,
                customer.Accomodation,
                customer.Language,
                customer.RecordingChannel,
                customer.CitizenshipCountryId,
                customer.AccomodationCountryId,
                Emails = customer.Emails.Select(e => e.Email).ToList()
            });
        }

        [HttpGet("persontypes")]
        public async Task<IActionResult> GetPersonTypes()
        {
            var types = await _context.PersonTypes
                .Select(pt => new { pt.Id, pt.Name })
                .ToListAsync();
            return Ok(types);
        }

        [HttpGet("taxoffices")]
        public async Task<IActionResult> GetTaxOffices()
        {
            var offices = await _context.TaxOffices
                .Select(t => new { t.Id, t.Name })
                .ToListAsync();
            return Ok(offices);
        }

        [HttpGet("citizenships")]
        public async Task<IActionResult> GetCitizenships()
        {
            var citizenships = await _context.Citizenships
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            return Ok(citizenships);
        }
        [HttpGet("accomodations")]
        public async Task<IActionResult> GetAccomodations()
        {
            var accomodations = await _context.Accomodations
                .Select(a => new { a.Id, a.Name })
                .ToListAsync();
            return Ok(accomodations);
        }

        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await _context.Languages
                .Select(l => new { l.Id, l.Name })
                .ToListAsync();
            return Ok(languages);
        }
    }
}
