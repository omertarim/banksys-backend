public class TransactionViewModel
{
    public string Direction { get; set; } = null!;
    public string CounterpartyName { get; set; } = null!;
    public string CounterpartyIban { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
}
