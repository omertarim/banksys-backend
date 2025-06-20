using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BankSysAPI.Models;
using BankSysAPI.Data;

namespace BankSysAPI.Controllers
{
    [ApiController]
    [Route("api/participation")]
    public class ParticipationLoanController : ControllerBase
    {
        private readonly BankingDbContext _context;

        private readonly Dictionary<string, decimal> _profitRates = new()
        {
            { "İhtiyaç", 2.4m },
            { "Taşıt", 2.1m },
            { "Konut", 1.7m }
        };

        public ParticipationLoanController(BankingDbContext context)
        {
            _context = context;
        }

        [HttpPost("calculate")]
        public IActionResult CalculateParticipationLoan([FromBody] ParticipationLoanRequest request)
        {
            if (request.Amount <= 0 || request.TermInMonths <= 0)
                return BadRequest("Geçersiz kredi tutarı veya vade.");

            if (!_profitRates.TryGetValue(request.LoanType, out var monthlyProfitRate))
                return BadRequest("Geçersiz kredi türü. Lütfen 'İhtiyaç', 'Taşıt' veya 'Konut' girin.");

            decimal totalProfit = request.Amount * monthlyProfitRate * request.TermInMonths / 100;
            decimal totalRepayment = request.Amount + totalProfit;
            decimal monthlyInstallment = totalRepayment / request.TermInMonths;

            var response = new ParticipationLoanResponse
            {
                TotalProfit = Math.Round(totalProfit, 2),
                TotalRepayment = Math.Round(totalRepayment, 2),
                MonthlyInstallment = Math.Round(monthlyInstallment, 2)
            };

            return Ok(response);
        }

        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> ApplyForLoan([FromBody] LoanApplicationRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var application = new LoanApplication
            {
                UserId = userId,
                LoanType = request.CreditType,
                Amount = request.Amount,
                TermInMonths = request.TermInMonths,
                Status = "Pending",
                ApplicationDate = DateTime.UtcNow
            };

            _context.LoanApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kredi başvurunuz alınmıştır ve onay bekliyor." });
        }
    }
}
