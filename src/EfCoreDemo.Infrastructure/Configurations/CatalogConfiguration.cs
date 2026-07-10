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
        // id gerado), o EF reserva um bloco com um único 'SELECT NEXT VALUE FOR' e
        // distribui os ids em memória. A sequence tem INCREMENT BY 10, então cada
        // NEXT VALUE FOR já devolve o primeiro id do bloco (1, 11, 21...) e o bloco
        // é [valor, valor + 9]. Efeito: o Id existe no Add(), antes do SaveChanges,
        // e o INSERT o envia explicitamente.
        //
        // Só funciona com PK inteira: sequences não geram Guid (ver Author, que usa
        // UUID v7 justamente por isso).
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
