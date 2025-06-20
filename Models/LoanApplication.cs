using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSysAPI.Models
{
    public class LoanApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public string LoanType { get; set; }  // "İhtiyaç", "Taşıt", "Konut"

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int TermInMonths { get; set; }

        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        

    }
}
