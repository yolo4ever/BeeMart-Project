using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Models;

[Table("LtPaymentMethod")]
public class LtPaymentMethod
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPaymentMethod { get; set; }

    [Required]
    [MaxLength(50)]
    public string MethodName { get; set; } = string.Empty;

    public bool Stsrc { get; set; } = true;

    public DateTime Datein { get; set; } = DateTime.UtcNow;

    [MaxLength(36)]
    public string Userin { get; set; } = "SYSTEM";

    public DateTime? Dateup { get; set; }

    [MaxLength(36)]
    public string? Userup { get; set; }

    public virtual ICollection<TrTransaction> Transactions { get; set; } = new List<TrTransaction>();
}
