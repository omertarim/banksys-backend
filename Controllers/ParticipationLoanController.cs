using Microsoft.AspNetCore.Mvc;
using BankSysAPI.Models;

namespace BankSysAPI.Controllers
{
    [ApiController]
    [Route("api/participation")]
    public class ParticipationLoanController : ControllerBase
    {
        private readonly Dictionary<string, decimal> _profitRates = new()
        {
            { "İhtiyaç", 2.4m },
            { "Taşıt", 2.1m },
            { "Konut", 1.7m }
        };

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
    }
}
