namespace BankSysAPI.Models
{
    public class CreateAccountRequest
    {
        public string AccountType { get; set; } = "Cari";
        public string Currency { get; set; } = "TL";
    }
}
