namespace BankSysAPI.Models
{
    public class CustomerLoginRequest
    {
        public string? CustomerNumber { get; set; }
        public int? CitizenshipCountryId { get; set; } // string DEĞİL
        public string Password { get; set; } = string.Empty;
    }

}
