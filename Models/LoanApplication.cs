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
        public decimal Amount { get; set; }

        [Required]
        public int TermInMonths { get; set; }

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        public int? TargetAccountId { get; set; }

        [ForeignKey("TargetAccountId")]
        public Account TargetAccount { get; set; }

        public int LoanApplicationTypeId { get; set; }
        public LoanApplicationType? LoanApplicationType { get; set; }

        public int LoanStatusId { get; set; }
        public LoanStatus LoanStatus { get; set; }





    }
}
