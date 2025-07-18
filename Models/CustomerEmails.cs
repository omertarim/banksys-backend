using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSysAPI.Models
{
    public class CustomerEmail
    {
        [Key]
        public int Id { get; set; }  // <-- Burada "EmailId" yerine "Id" kullanılmalı

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } = null!;
    }
}
