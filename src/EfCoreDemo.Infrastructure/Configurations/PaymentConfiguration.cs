using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreDemo.Infrastructure.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // HERANÇA TPT (Table-per-Type): tabela base + uma tabela por subtipo.
        builder.UseTptMappingStrategy();
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasValueGenerator<UuidV7ValueGenerator>()
            .ValueGeneratedOnAdd();

        // decimal simples: complex type em TPT quebra na consulta (limitação do EF Core).
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3);

        // 1:1 com Order — a FK fica em Payment.OrderId.
        builder.HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.OrderId).IsUnique();
    }
}

public class CreditCardPaymentConfiguration : IEntityTypeConfiguration<CreditCardPayment>
{
    public void Configure(EntityTypeBuilder<CreditCardPayment> builder)
    {
        builder.ToTable("CreditCardPayments");
        builder.Property(p => p.CardLast4).HasMaxLength(4);
        builder.Property(p => p.Brand).HasMaxLength(30);
    }
}

public class PixPaymentConfiguration : IEntityTypeConfiguration<PixPayment>
{
    public void Configure(EntityTypeBuilder<PixPayment> builder)
    {
        builder.ToTable("PixPayments");
        builder.Property(p => p.PixKey).HasMaxLength(140);
        builder.Property(p => p.TransactionId).HasMaxLength(60);
    }
}

public class BoletoPaymentConfiguration : IEntityTypeConfiguration<BoletoPayment>
{
    public void Configure(EntityTypeBuilder<BoletoPayment> builder)
    {
        builder.ToTable("BoletoPayments");
        builder.Property(p => p.Barcode).HasMaxLength(60);
    }
}
