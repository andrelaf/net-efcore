using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreDemo.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasValueGenerator<UuidV7ValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        // Enum persistido como texto.
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(o => o.ConcurrencyToken).IsConcurrencyToken();

        // Complex Type: total do pedido.
        builder.ComplexProperty(o => o.Total, p =>
        {
            p.Property(m => m.Amount).HasColumnName("Total_Amount").HasPrecision(18, 2);
            p.Property(m => m.Currency).HasColumnName("Total_Currency").HasMaxLength(3);
        });

        // Owned Type: endereço de entrega (table splitting com prefixo Ship_).
        builder.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(x => x.Street).HasColumnName("Ship_Street").HasMaxLength(200);
            a.Property(x => x.Number).HasColumnName("Ship_Number").HasMaxLength(20);
            a.Property(x => x.City).HasColumnName("Ship_City").HasMaxLength(120);
            a.Property(x => x.State).HasColumnName("Ship_State").HasMaxLength(2);
            a.Property(x => x.ZipCode).HasColumnName("Ship_ZipCode").HasMaxLength(9);
            a.Property(x => x.Country).HasColumnName("Ship_Country").HasMaxLength(60);
        });

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter("SoftDelete", o => !o.IsDeleted);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasValueGenerator<UuidV7ValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Ignore(i => i.LineTotal); // propriedade calculada (não mapeada)

        builder.ComplexProperty(i => i.UnitPrice, p =>
        {
            p.Property(m => m.Amount).HasColumnName("UnitPrice_Amount").HasPrecision(18, 2);
            p.Property(m => m.Currency).HasColumnName("UnitPrice_Currency").HasMaxLength(3);
        });

        builder.HasOne(i => i.Book)
            .WithMany(b => b.OrderItems)
            .HasForeignKey(i => i.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
