namespace BankSysAPI.Models
{
    public class LoanApplicationRequest
    {
        public int LoanApplicationTypeId { get; set; } // ✅ string değil
        public decimal Amount { get; set; }
        public int TermInMonths { get; set; }
        public int TargetAccountId { get; set; }
    }
}
