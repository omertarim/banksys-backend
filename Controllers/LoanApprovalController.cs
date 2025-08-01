using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankSysAPI.Data;
using BankSysAPI.Models;
using BankSysAPI.Services;

namespace BankSysAPI.Controllers
{
     public class LoanStatusUpdateRequest
    {
        public string NewStatus { get; set; }
    }
    [ApiController]
    [Route("api/admin/loans")]
    [Authorize(Roles = "Admin")]
    public class LoanApprovalController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly LoanScoreService _loanScoreService;

        public LoanApprovalController(BankingDbContext context, LoanScoreService loanScoreService)
        {
            _context = context;
            _loanScoreService = loanScoreService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApplications()
        {
            var pendingLoans = await _context.LoanApplications
                .Include(l => l.User)
                .Include(l => l.LoanApplicationType)
                .Where(l => l.LoanStatusId == -1) // Use LoanStatusId instead of Status
                .ToListAsync();

            var loansWithScores = new List<object>();

            foreach (var loan in pendingLoans)
            {
                var loanScore = await _context.LoanScores
                    .Include(ls => ls.RiskLevel)
                    .Include(ls => ls.RecommendedApproval)
                    .Include(ls => ls.RecommendationStatus)
                    .FirstOrDefaultAsync(ls => ls.LoanApplicationId == loan.Id);

                if (loanScore == null)
                {
                    try
                    {
                        loanScore = await _loanScoreService.CalculateLoanScoreAsync(loan.Id);
                        _context.LoanScores.Add(loanScore);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                      
                        var defaultRiskLevel = await _context.RiskLevels.FirstOrDefaultAsync(r => r.Name == "Medium");
                        var defaultRecommendedApproval = await _context.RecommendedApprovals.FirstOrDefaultAsync(r => r.Name == "Rejected");
                        var defaultRecommendationStatus = await _context.RecommendationStatuses.FirstOrDefaultAsync(r => r.Name == "Score calculation failed");

                        loanScore = new LoanScore
                        {
                            LoanApplicationId = loan.Id,
                            UserId = loan.UserId,
                            FinalScore = 50,
                            RiskLevelId = defaultRiskLevel?.Id ?? 2,
                            RecommendedApprovalId = defaultRecommendedApproval?.Id ?? 2,
                            RecommendationStatusId = defaultRecommendationStatus?.Id ?? 5
                        };
                    }
                }

                loansWithScores.Add(new
                {
                    loan.Id,
                    loan.Amount,
                    loan.TermInMonths,
                    loan.ApplicationDate,
                    loan.LoanApplicationType.Name,
                    user = new { loan.User.FullName, loan.User.Email },
                    score = new
                    {
                        loanScore.AverageMonthlyBalance,
                        loanScore.MonthlyIncome,
                        loanScore.DebtToIncomeRatio,
                        loanScore.PreviousLoanApplications,
                        loanScore.FinalScore,
                        riskLevelId = loanScore.RiskLevelId,
                        riskLevelName = loanScore.RiskLevel?.Name,
                        recommendedApprovalId = loanScore.RecommendedApprovalId,
                        recommendedApprovalName = loanScore.RecommendedApproval?.Name,
                        recommendationStatusId = loanScore.RecommendationStatusId,
                        recommendationStatusName = loanScore.RecommendationStatus?.Name
                    }
                });
            }

            return Ok(loansWithScores);
        }

        [HttpPut("{id}/update-status")]
        public async Task<IActionResult> UpdateLoanStatus(int id, [FromBody] LoanStatusUpdateRequest request)
        {
            var loan = await _context.LoanApplications.FindAsync(id);
            if (loan == null)
                return NotFound("Başvuru bulunamadı.");

            if (request.NewStatus != "Approved" && request.NewStatus != "Rejected")
                return BadRequest("Geçersiz durum.");

            // Get or calculate loan score
            var loanScore = await _context.LoanScores
                .Include(ls => ls.RiskLevel)
                .Include(ls => ls.RecommendedApproval)
                .Include(ls => ls.RecommendationStatus)
                .FirstOrDefaultAsync(ls => ls.LoanApplicationId == id);

            if (loanScore == null)
            {
                try
                {
                    loanScore = await _loanScoreService.CalculateLoanScoreAsync(id);
                    _context.LoanScores.Add(loanScore);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Score calculation failed: {ex.Message}");
                }
            }

            if (request.NewStatus == "Approved")
            {
                loan.LoanStatusId = -2; // -2  "Approved"
                var targetAccount = await _context.Accounts.FindAsync(loan.TargetAccountId);
                if (targetAccount == null)
                    return NotFound("Hedef hesap bulunamadı.");

                targetAccount.Balance += loan.Amount;
            }
            else if (request.NewStatus == "Rejected")
            {
                loan.LoanStatusId = -3; // -3 is "Rejected"
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"Başvuru {request.NewStatus.ToLower()} olarak güncellendi.",
                score = new
                {
                    loanScore.FinalScore,
                    riskLevelId = loanScore.RiskLevelId,
                    riskLevelName = loanScore.RiskLevel?.Name,
                    recommendedApprovalId = loanScore.RecommendedApprovalId,
                    recommendedApprovalName = loanScore.RecommendedApproval?.Name,
                    recommendationStatusId = loanScore.RecommendationStatusId,
                    recommendationStatusName = loanScore.RecommendationStatus?.Name
                }
            });
        }

        // GET: Get detailed loan score for a specific application
        [HttpGet("{id}/score")]
        public async Task<IActionResult> GetLoanScore(int id)
        {
            var loan = await _context.LoanApplications
                .Include(l => l.User)
                .Include(l => l.LoanApplicationType)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
                return NotFound("Loan application not found.");

            var loanScore = await _context.LoanScores
                .Include(ls => ls.RiskLevel)
                .Include(ls => ls.RecommendedApproval)
                .Include(ls => ls.RecommendationStatus)
                .FirstOrDefaultAsync(ls => ls.LoanApplicationId == id);

            if (loanScore == null)
            {
                try
                {
                    loanScore = await _loanScoreService.CalculateLoanScoreAsync(id);
                    _context.LoanScores.Add(loanScore);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return BadRequest($"Score calculation failed: {ex.Message}");
                }
            }

            var detailedScore = new
            {
                loanApplication = new
                {
                    loan.Id,
                    loan.Amount,
                    loan.TermInMonths,
                    loan.ApplicationDate,
                    loanType = loan.LoanApplicationType?.Name,
                    user = new { loan.User.FullName, loan.User.Email }
                },
                score = new
                {
                    loanScore.AverageMonthlyBalance,
                    loanScore.MonthlyIncome,
                    loanScore.DebtToIncomeRatio,
                    loanScore.PreviousLoanApplications,
                    loanScore.FinalScore,
                    riskLevelId = loanScore.RiskLevelId,
                    riskLevelName = loanScore.RiskLevel?.Name,
                    recommendedApprovalId = loanScore.RecommendedApprovalId,
                    recommendedApprovalName = loanScore.RecommendedApproval?.Name,
                    recommendationStatusId = loanScore.RecommendationStatusId,
                    recommendationStatusName = loanScore.RecommendationStatus?.Name,
                    loanScore.CalculatedAt
                }
            };

            return Ok(detailedScore);
        }

        // POST: Recalculate loan score for a specific application
        [HttpPost("{id}/recalculate-score")]
        public async Task<IActionResult> RecalculateLoanScore(int id)
        {
            var loan = await _context.LoanApplications.FindAsync(id);
            if (loan == null)
                return NotFound("Loan application not found.");

            try
            {
                var loanScore = await _loanScoreService.CalculateLoanScoreAsync(id);
                
                // Update existing score or create new one
                var existingScore = await _context.LoanScores
                    .Include(ls => ls.RiskLevel)
                    .Include(ls => ls.RecommendedApproval)
                    .Include(ls => ls.RecommendationStatus)
                    .FirstOrDefaultAsync(ls => ls.LoanApplicationId == id);

                if (existingScore != null)
                {
                    existingScore.AverageMonthlyBalance = loanScore.AverageMonthlyBalance;
                    existingScore.MonthlyIncome = loanScore.MonthlyIncome;
                    existingScore.DebtToIncomeRatio = loanScore.DebtToIncomeRatio;
                    existingScore.PreviousLoanApplications = loanScore.PreviousLoanApplications;
                    existingScore.FinalScore = loanScore.FinalScore;
                    existingScore.RiskLevelId = loanScore.RiskLevelId;
                    existingScore.RecommendedApprovalId = loanScore.RecommendedApprovalId;
                    existingScore.RecommendationStatusId = loanScore.RecommendationStatusId;
                    existingScore.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.LoanScores.Add(loanScore);
                }

                await _context.SaveChangesAsync();

                // Reload the score with includes for response
                var updatedScore = await _context.LoanScores
                    .Include(ls => ls.RiskLevel)
                    .Include(ls => ls.RecommendedApproval)
                    .Include(ls => ls.RecommendationStatus)
                    .FirstOrDefaultAsync(ls => ls.LoanApplicationId == id);

                return Ok(new { 
                    message = "Loan score recalculated successfully",
                    score = new
                    {
                        updatedScore.AverageMonthlyBalance,
                        updatedScore.MonthlyIncome,
                        updatedScore.DebtToIncomeRatio,
                        updatedScore.PreviousLoanApplications,
                        updatedScore.FinalScore,
                        riskLevelId = updatedScore.RiskLevelId,
                        riskLevelName = updatedScore.RiskLevel?.Name,
                        recommendedApprovalId = updatedScore.RecommendedApprovalId,
                        recommendedApprovalName = updatedScore.RecommendedApproval?.Name,
                        recommendationStatusId = updatedScore.RecommendationStatusId,
                        recommendationStatusName = updatedScore.RecommendationStatus?.Name
                    }
                });

                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Loan score recalculated successfully",
                    score = new
                    {
                        loanScore.AverageMonthlyBalance,
                        loanScore.MonthlyIncome,
                        loanScore.DebtToIncomeRatio,
                        loanScore.PreviousLoanApplications,
                        loanScore.FinalScore,
                        riskLevelId = loanScore.RiskLevelId,
                        riskLevelName = loanScore.RiskLevel?.Name,
                        recommendedApprovalId = loanScore.RecommendedApprovalId,
                        recommendedApprovalName = loanScore.RecommendedApproval?.Name,
                        recommendationStatusId = loanScore.RecommendationStatusId,
                        recommendationStatusName = loanScore.RecommendationStatus?.Name
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Score recalculation failed: {ex.Message}");
            }
        }
    }
}
