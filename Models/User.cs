using System.ComponentModel.DataAnnotations;

namespace BankSysAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        // Navigation property (opsiyonel ama iyi olur)
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
