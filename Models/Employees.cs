using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSysAPI.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign Key

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Surname { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public bool IsActive { get; set; } = true;

        public int RoleId { get; set; }

        public string Gender { get; set; } = string.Empty;

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public string CreateUser { get; set; } = string.Empty;

        public DateTime LastUpdateDate { get; set; } = DateTime.UtcNow;

        public string LastUpdateUser { get; set; } = string.Empty;

        public string HostIp { get; set; } = string.Empty;

        [NotMapped]
        public string Email { get; set; } = string.Empty;
    }
}
