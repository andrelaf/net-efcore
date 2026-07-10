using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreDemo.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        // PK int gerada no CLIENTE pelo algoritmo Hi/Lo, apoiado em uma SEQUENCE
        // do SQL Server. Em vez de IDENTITY (um round-trip por insert para ler o
        // id gerado), o EF reserva um bloco de ids com um único
        // 'SELECT NEXT VALUE FOR' e distribui o 'lo' em memória. Efeito: o Id já
        // existe no Add(), antes do SaveChanges, e o INSERT o envia explicitamente.
        builder.Property(c => c.Id).UseHiLo("CategoryHiLoSequence");

        builder.Property(c => c.Name).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(140).IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique();

        // Auto-relacionamento: uma categoria pode ter uma categoria pai e filhos.
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasValueGenerator<UuidV7ValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Bio).HasMaxLength(2000);
        builder.Property(a => a.Country).HasMaxLength(60);
    }
}
