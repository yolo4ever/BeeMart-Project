using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Models;

[Table("TrTransactionDetail")]
public class TrTransactionDetail
{
    [Key]
    [MaxLength(36)]
    public string IdTransactionDetail { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(36)]
    [ForeignKey(nameof(Transaction))]
    public string IdTransaction { get; set; } = string.Empty;

    [Required]
    [MaxLength(36)]
    [ForeignKey(nameof(Product))]
    public string IdProduct { get; set; } = string.Empty;

    public int Qty { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Discount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    public bool Stsrc { get; set; } = true;

    public DateTime Datein { get; set; } = DateTime.UtcNow;

    [MaxLength(36)]
    public string Userin { get; set; } = "SYSTEM";

    public DateTime? Dateup { get; set; }

    [MaxLength(36)]
    public string? Userup { get; set; }

    public virtual TrTransaction? Transaction { get; set; }
    public virtual MsProduct? Product { get; set; }
}
