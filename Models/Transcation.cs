using BankSysAPI.Models; // <-- BU SATIR EKLENDİ

namespace BankSysAPI.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int? SenderAccountId { get; set; }
        public Account? SenderAccount { get; set; }  // navigation

        public int ReceiverAccountId { get; set; }
        public Account ReceiverAccount { get; set; } = null!;

        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    

    }
}
