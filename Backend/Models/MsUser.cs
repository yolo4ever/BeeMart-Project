using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Models;

[Table("MsUser")]
public class MsUser
{
    [Key]
    [MaxLength(5)]
    public string IdUser { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Cashier"; // "Admin" or "Cashier"

    public bool Stsrc { get; set; } = true;

    public DateTime Datein { get; set; } = DateTime.UtcNow;

    [MaxLength(36)]
    public string Userin { get; set; } = "SYSTEM";

    public DateTime? Dateup { get; set; }

    [MaxLength(36)]
    public string? Userup { get; set; }

    public virtual ICollection<TrTransaction> Transactions { get; set; } = new List<TrTransaction>();
}
