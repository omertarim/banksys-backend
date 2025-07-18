// Dosya: Models/LoanApplicationType.cs

namespace BankSysAPI.Models
{
    public class LoanApplicationType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
