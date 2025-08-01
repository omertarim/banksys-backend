using BankSysAPI.Data;
using BankSysAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSysAPI.Services
{
    public class LoanScoreService
    {
        private readonly BankingDbContext _context;

        public LoanScoreService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<LoanScore> CalculateLoanScoreAsync(int loanApplicationId)
        {
            try
            {
                Console.WriteLine($"Starting score calculation for loan application {loanApplicationId}");
                
                var loanApplication = await _context.LoanApplications
                    .Include(l => l.User)
                    .FirstOrDefaultAsync(l => l.Id == loanApplicationId);

                if (loanApplication == null)
                {
                    Console.WriteLine($"Loan application {loanApplicationId} not found");
                    throw new ArgumentException("Loan application not found");
                }

                var userId = loanApplication.UserId;
                Console.WriteLine($"Found loan application for user {userId}");

                var accounts = await _context.Accounts
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

//son 6 aydakki işlemleri aldım
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                var transactions = await _context.Transactions
                    .Include(t => t.SenderAccount)
                    .Include(t => t.ReceiverAccount)
                    .Where(t => (t.SenderAccount != null && t.SenderAccount.UserId == userId) || 
                               (t.ReceiverAccount != null && t.ReceiverAccount.UserId == userId))
                    .Where(t => t.Timestamp >= sixMonthsAgo)

                    .ToListAsync();

             //önceki kredi başvurularına bakıyorum
                var previousApplications = await _context.LoanApplications
                    .Where(l => l.UserId == userId && l.Id != loanApplicationId)
                    .CountAsync();

                decimal averageMonthlyBalance = accounts.Any()
                    ? accounts.Sum(a => a.Balance) / accounts.Count
                    : 0m;

                var monthlyIncome = 0m;
                if (transactions.Any())
                {
                    var incomingTransactions = transactions
                        .Where(t => t.ReceiverAccount != null && t.ReceiverAccount.UserId == userId)
                        .ToList();

                    if (incomingTransactions.Any())
                    {
                        var monthlyGroups = incomingTransactions
                            .GroupBy(t => new { t.Timestamp.Year, t.Timestamp.Month })
                            .ToList();
                        
                        if (monthlyGroups.Any())
                        {
                            monthlyIncome = monthlyGroups.Average(g => g.Sum(t => t.Amount));
                        }
                    }
                }

                decimal debtToIncomeRatio = (monthlyIncome > 0 && loanApplication.TermInMonths > 0)
                    ? loanApplication.Amount / (monthlyIncome * loanApplication.TermInMonths)
                    : 1m;

            
            var riskLevels = await _context.RiskLevels.ToListAsync();
            var recommendedApprovals = await _context.RecommendedApprovals.ToListAsync();
            var recommendationStatuses = await _context.RecommendationStatuses.ToListAsync();

            Console.WriteLine($"Found {riskLevels.Count} risk levels, {recommendedApprovals.Count} recommended approvals, {recommendationStatuses.Count} recommendation statuses");

            int riskLevelId;
            decimal finalScore;
            int recommendedApprovalId;
            int recommendationStatusId;

            if (averageMonthlyBalance < 1000m || monthlyIncome < 2000m)
            {
                riskLevelId = riskLevels.First(r => r.Name == "High").Id;
                finalScore = 40;
                recommendedApprovalId = recommendedApprovals.First(r => r.Name == "Rejected").Id;
                recommendationStatusId = recommendationStatuses.First(r => r.Name == "Low balance or income").Id;
            }
            else if (debtToIncomeRatio > 0.5m)
            {
                riskLevelId = riskLevels.First(r => r.Name == "Medium").Id;
                finalScore = 60;
                recommendedApprovalId = recommendedApprovals.First(r => r.Name == "Rejected").Id;
                recommendationStatusId = recommendationStatuses.First(r => r.Name == "High debt-to-income ratio").Id;
            }
            else if (previousApplications > 2)
            {
                riskLevelId = riskLevels.First(r => r.Name == "Medium").Id;
                finalScore = 65;
                recommendedApprovalId = recommendedApprovals.First(r => r.Name == "Rejected").Id;
                recommendationStatusId = recommendationStatuses.First(r => r.Name == "Too many previous loan applications").Id;
            }
            else
            {
                riskLevelId = riskLevels.First(r => r.Name == "Low").Id;
                finalScore = 85;
                recommendedApprovalId = recommendedApprovals.First(r => r.Name == "Approved").Id;
                recommendationStatusId = recommendationStatuses.First(r => r.Name == "Good financial standing").Id;
            }

            var loanScore = new LoanScore
            {
                LoanApplicationId = loanApplicationId,
                UserId = userId,
                AverageMonthlyBalance = averageMonthlyBalance,
                MonthlyIncome = monthlyIncome,
                DebtToIncomeRatio = debtToIncomeRatio,
                PreviousLoanApplications = previousApplications,
                FinalScore = finalScore,
                RiskLevelId = riskLevelId,
                RecommendedApprovalId = recommendedApprovalId,
                RecommendationStatusId = recommendationStatusId,
                CalculatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Console.WriteLine($"Created LoanScore: RiskLevelId={riskLevelId}, RecommendedApprovalId={recommendedApprovalId}, RecommendationStatusId={recommendationStatusId}");

            return loanScore;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Score calculation failed: {ex.Message}");
            }
        }
    }
} 