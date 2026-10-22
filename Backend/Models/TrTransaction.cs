using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Models;

[Table("TrTransaction")]
public class TrTransaction
{
    [Key]
    [MaxLength(36)]
    public string IdTransaction { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(20)]
    public string InvoiceNo { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(5)]
    [ForeignKey(nameof(User))]
    public string IdUser { get; set; } = string.Empty;

    [ForeignKey(nameof(PaymentMethod))]
    public int IdPaymentMethod { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public bool Stsrc { get; set; } = true;

    public DateTime Datein { get; set; } = DateTime.UtcNow;

    [MaxLength(36)]
    public string Userin { get; set; } = "SYSTEM";

    public DateTime? Dateup { get; set; }

    [MaxLength(36)]
    public string? Userup { get; set; }

    public virtual MsUser? User { get; set; }
    public virtual LtPaymentMethod? PaymentMethod { get; set; }
    public virtual ICollection<TrTransactionDetail> TransactionDetails { get; set; } = new List<TrTransactionDetail>();
}
