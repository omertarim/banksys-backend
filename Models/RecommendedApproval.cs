using System.ComponentModel.DataAnnotations;

namespace BankSysAPI.Models
{
    public class RecommendedApproval
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
} 