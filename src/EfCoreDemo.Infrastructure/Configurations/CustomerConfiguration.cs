using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreDemo.Infrastructure.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        // PK Guid gerada como UUID v7 pela aplicação.
        builder.Property(c => c.Id)
            .HasValueGenerator<UuidV7ValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(c => c.FullName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(c => c.Email).IsUnique();

        // Token de concorrência otimista (regenerado pelo interceptor de auditoria).
        builder.Property(c => c.RowVersion).IsRowVersion();

        // Complex Type: Address vira colunas Address_* na própria tabela.
        builder.ComplexProperty(c => c.Address, a =>
        {
            a.Property(x => x.Street).HasColumnName("Address_Street").HasMaxLength(200);
            a.Property(x => x.Number).HasColumnName("Address_Number").HasMaxLength(20);
            a.Property(x => x.City).HasColumnName("Address_City").HasMaxLength(120);
            a.Property(x => x.State).HasColumnName("Address_State").HasMaxLength(2);
            a.Property(x => x.ZipCode).HasColumnName("Address_ZipCode").HasMaxLength(9);
            a.Property(x => x.Country).HasColumnName("Address_Country").HasMaxLength(60);
        });

        // Global Query Filter NOMEADO (EF Core 10): esconde clientes excluídos.
        builder.HasQueryFilter("SoftDelete", c => !c.IsDeleted);

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.ToTable("CustomerProfiles");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasValueGenerator<UuidV7ValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Bio).HasMaxLength(1000);
        builder.Property(p => p.PreferredLanguage).HasMaxLength(10);

        // Coleção primitiva -> coluna JSON (recurso nativo do EF Core).
        builder.PrimitiveCollection(p => p.Interests);

        // Relacionamento 1:1 — FK em CustomerProfile.CustomerId.
        builder.HasOne(p => p.Customer)
            .WithOne(c => c.Profile)
            .HasForeignKey<CustomerProfile>(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.CustomerId).IsUnique();
    }
}
