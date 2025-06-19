namespace BankSysAPI.Models
{
    public class AdminDepositRequest
    {
        public string IBAN { get; set; }
        public decimal Amount { get; set; }
    }
}
