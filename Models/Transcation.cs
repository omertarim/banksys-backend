namespace BankSysAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int? SenderAccountId { get; set; }  // nullable
        public int ReceiverAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
