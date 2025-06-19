namespace BankSysAPI.Models
{
    public class ParticipationLoanRequest
    {
        public decimal Amount { get; set; }         // Ana para (TL)
        public int TermInMonths { get; set; }        // Vade (ay)
        public string LoanType { get; set; } = null!; // Kredi türü: "İhtiyaç", "Taşıt", "Konut"
    }
}
