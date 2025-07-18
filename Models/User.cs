using System.ComponentModel.DataAnnotations;
using BankSysAPI.Models;


public class User
{
    public int Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpires { get; set; }

    public bool IsApproved { get; set; } = false;
    //public bool IsAdmin { get; set; } = false;

    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";

    public Role RoleId { get; set; } = Role.Customer;
    public bool IsActive { get; set; } = true;
    public string? HostIp { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
