using BeeMart.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Data;

public class BeeMartDbContext : DbContext
{
    public BeeMartDbContext(DbContextOptions<BeeMartDbContext> options) : base(options)
    {
    }

    public DbSet<LtPaymentMethod> LtPaymentMethods => Set<LtPaymentMethod>();
    public DbSet<MsUser> MsUsers => Set<MsUser>();
    public DbSet<MsProduct> MsProducts => Set<MsProduct>();
    public DbSet<TrTransaction> TrTransactions => Set<TrTransaction>();
    public DbSet<TrTransactionDetail> TrTransactionDetails => Set<TrTransactionDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // LtPaymentMethod
        modelBuilder.Entity<LtPaymentMethod>(entity =>
        {
            entity.HasKey(e => e.IdPaymentMethod);
            entity.Property(e => e.IdPaymentMethod).ValueGeneratedOnAdd();
            entity.Property(e => e.MethodName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Userin).HasMaxLength(36).IsRequired();
            entity.Property(e => e.Userup).HasMaxLength(36);
        });

        // MsUser
        modelBuilder.Entity<MsUser>(entity =>
        {
            entity.HasKey(e => e.IdUser);
            entity.Property(e => e.IdUser).HasMaxLength(5).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.FullName).HasMaxLength(300).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Userin).HasMaxLength(36).IsRequired();
            entity.Property(e => e.Userup).HasMaxLength(36);
        });

        // MsProduct
        modelBuilder.Entity<MsProduct>(entity =>
        {
            entity.HasKey(e => e.IdProduct);
            entity.Property(e => e.IdProduct).HasMaxLength(36).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(5,2)").HasDefaultValue(0);
            entity.Property(e => e.Userin).HasMaxLength(36).IsRequired();
            entity.Property(e => e.Userup).HasMaxLength(36);
        });

        // TrTransaction
        modelBuilder.Entity<TrTransaction>(entity =>
        {
            entity.HasKey(e => e.IdTransaction);
            entity.Property(e => e.IdTransaction).HasMaxLength(36).IsRequired();
            entity.Property(e => e.InvoiceNo).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.InvoiceNo).IsUnique();
            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => e.IdUser);
            entity.HasIndex(e => e.IdPaymentMethod);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Userin).HasMaxLength(36).IsRequired();
            entity.Property(e => e.Userup).HasMaxLength(36);

            entity.HasOne(d => d.User)
                  .WithMany(p => p.Transactions)
                  .HasForeignKey(d => d.IdUser)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.PaymentMethod)
                  .WithMany(p => p.Transactions)
                  .HasForeignKey(d => d.IdPaymentMethod)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TrTransactionDetail
        modelBuilder.Entity<TrTransactionDetail>(entity =>
        {
            entity.HasKey(e => e.IdTransactionDetail);
            entity.Property(e => e.IdTransactionDetail).HasMaxLength(36).IsRequired();
            entity.Property(e => e.IdTransaction).HasMaxLength(36).IsRequired();
            entity.Property(e => e.IdProduct).HasMaxLength(36).IsRequired();
            entity.HasIndex(e => e.IdTransaction);
            entity.HasIndex(e => e.IdProduct);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(5,2)").HasDefaultValue(0);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Userin).HasMaxLength(36).IsRequired();
            entity.Property(e => e.Userup).HasMaxLength(36);

            entity.HasOne(d => d.Transaction)
                  .WithMany(p => p.TransactionDetails)
                  .HasForeignKey(d => d.IdTransaction)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Product)
                  .WithMany(p => p.TransactionDetails)
                  .HasForeignKey(d => d.IdProduct)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
