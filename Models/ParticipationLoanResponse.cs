namespace BankSysAPI.Models
{
    public class ParticipationLoanResponse
    {
        public decimal TotalProfit { get; set; }           // Toplam kâr
        public decimal TotalRepayment { get; set; }        // Geri ödenecek toplam tutar
        public decimal MonthlyInstallment { get; set; }    // Aylık taksit
    }
}
