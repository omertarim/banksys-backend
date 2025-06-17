using System.ComponentModel.DataAnnotations;

namespace BankSysAPI.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        public string IBAN { get; set; } = string.Empty;

        [Required]
        public string AccountNumber { get; set; } = string.Empty;

        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required]
        public string AccountType { get; set; } = "Cari Hesap"; // Default

        [Required]
        public string Currency { get; set; } = "TRY"; // Şimdilik sadece TL
    }
}
