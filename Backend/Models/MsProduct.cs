using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Models;

[Table("MsProduct")]
public class MsProduct
{
    [Key]
    [MaxLength(36)]
    public string IdProduct { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Weight { get; set; }

    public int Stock { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Discount { get; set; } = 0;

    public bool Stsrc { get; set; } = true;

    public DateTime Datein { get; set; } = DateTime.UtcNow;

    [MaxLength(36)]
    public string Userin { get; set; } = "SYSTEM";

    public DateTime? Dateup { get; set; }

    [MaxLength(36)]
    public string? Userup { get; set; }

    public virtual ICollection<TrTransactionDetail> TransactionDetails { get; set; } = new List<TrTransactionDetail>();
}
