using System.ComponentModel.DataAnnotations;

namespace BankSysAPI.Models
{
    public class LoanScore
    {
        [Key]
        public int Id { get; set; }

        public int LoanApplicationId { get; set; }
        public LoanApplication LoanApplication { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        // Only 4 metrics
        public decimal AverageMonthlyBalance { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public int PreviousLoanApplications { get; set; }

        // Results
        public decimal FinalScore { get; set; }
        
        // Foreign Keys - Sadece ID'ler
        public int RiskLevelId { get; set; }
        public RiskLevel RiskLevel { get; set; }
        
        public int RecommendedApprovalId { get; set; }
        public RecommendedApproval RecommendedApproval { get; set; }
        
        public int RecommendationStatusId { get; set; }
        public RecommendationStatus RecommendationStatus { get; set; }
        
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 