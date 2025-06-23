public class LoanApplicationRequest
{
    public string CreditType { get; set; }
    public decimal Amount { get; set; }
    public int TermInMonths { get; set; }
    public int TargetAccountId { get; set; } 
}
