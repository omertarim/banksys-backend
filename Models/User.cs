using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Username { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpires { get; set; }

    public bool IsApproved { get; set; }
    public bool IsAdmin { get; set; } = false;

    public string FullName { get; set; } = "";
    
    public string Status { get; set; } = "Pending"; // Varsayılan değer





}
