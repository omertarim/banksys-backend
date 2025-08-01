using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BankSysAPI.Models;
using BankSysAPI.Data;
using Microsoft.EntityFrameworkCore;
using BankSysAPI.Services;


namespace BankSysAPI.Controllers
{
    [ApiController]
    [Route("api/participation")]
    public class ParticipationLoanController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly LoanScoreService _loanScoreService;

        private readonly Dictionary<string, decimal> _profitRates = new()
        {
            { "İhtiyaç", 2.4m },
            { "Taşıt", 2.1m },
            { "Konut", 1.7m }
        };

        public ParticipationLoanController(BankingDbContext context, LoanScoreService loanScoreService)
        {
            _context = context;
            _loanScoreService = loanScoreService;
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

            // Kredi türü geçerli mi kontrol et
            var loanType = await _context.LoanApplicationTypes.FindAsync(request.LoanApplicationTypeId);
            if (loanType == null || !loanType.IsActive)
                return BadRequest("Geçersiz kredi türü.");

            var application = new LoanApplication
            {
                UserId = userId,
                LoanApplicationTypeId = request.LoanApplicationTypeId,
                Amount = request.Amount,
                TermInMonths = request.TermInMonths,
                LoanStatusId = -1, // Set to -1 for "Pending"
                ApplicationDate = DateTime.UtcNow,
                TargetAccountId = request.TargetAccountId
            };

            _context.LoanApplications.Add(application);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Created loan application with ID: {application.Id}");

            // Calculate loan score for the new application
            try
            {
                Console.WriteLine($"Starting score calculation for application {application.Id}");
                var loanScore = await _loanScoreService.CalculateLoanScoreAsync(application.Id);
                Console.WriteLine($"Score calculated successfully for application {application.Id}");
                _context.LoanScores.Add(loanScore);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Score saved to database for application {application.Id}");
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the application
                Console.WriteLine($"Failed to calculate loan score for application {application.Id}: {ex.Message}");
                Console.WriteLine($"Exception details: {ex.ToString()}");
            }

            return Ok(new { 
                message = "Kredi başvurunuz alınmıştır ve onay bekliyor.",
                applicationId = application.Id
            });
        }






        [Authorize]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyLoanApplications()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var applications = await _context.LoanApplications
                .Where(l => l.UserId == userId)
                .Include(l => l.LoanStatus)
                .Include(l => l.LoanApplicationType)
                .Select(l => new {
                    id = l.Id,
                    creditType = l.LoanApplicationType.Name,
                    amount = l.Amount,
                    termInMonths = l.TermInMonths,
                    status = l.LoanStatus.Name,
                    createdAt = l.ApplicationDate
                })
                .ToListAsync();

            return Ok(applications);
        }

    }
}
