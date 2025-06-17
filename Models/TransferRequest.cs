namespace BankSysAPI.Models
{
    public class TransferRequest
    {
        public string ReceiverIban { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}
