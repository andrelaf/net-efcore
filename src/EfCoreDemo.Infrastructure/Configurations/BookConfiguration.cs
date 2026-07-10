using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreDemo.Infrastructure.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasValueGenerator<UuidV7ValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(b => b.Title).HasMaxLength(300).IsRequired();
        builder.Property(b => b.Isbn).HasMaxLength(20).IsRequired();
        builder.HasIndex(b => b.Isbn).IsUnique();

        builder.Property(b => b.RowVersion).IsRowVersion();

        // HERANÇA TPH: PhysicalBook e EBook na mesma tabela, separados pela
        // coluna discriminadora "BookType".
        builder.HasDiscriminator<string>("BookType")
            .HasValue<PhysicalBook>("physical")
            .HasValue<EBook>("ebook");

        // Complex Type: Money -> colunas Price_Amount / Price_Currency.
        builder.ComplexProperty(b => b.Price, p =>
        {
            p.Property(m => m.Amount).HasColumnName("Price_Amount").HasPrecision(18, 2);
            p.Property(m => m.Currency).HasColumnName("Price_Currency").HasMaxLength(3);
        });

        // Complex Type serializado em JSON (EF Core 10): ComplexProperty(...).ToJson().
        builder.ComplexProperty(b => b.Metadata).ToJson("Metadata");

        // Coleção primitiva -> coluna JSON.
        builder.PrimitiveCollection(b => b.Tags);

        // N:1 com categoria.
        builder.HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Filtro nomeado de soft delete.
        builder.HasQueryFilter("SoftDelete", b => !b.IsDeleted);
    }
}

public class PhysicalBookConfiguration : IEntityTypeConfiguration<PhysicalBook>
{
    public void Configure(EntityTypeBuilder<PhysicalBook> builder)
    {
        // Owned Type: Dimensions -> colunas Dim_* na mesma tabela (table splitting).
        builder.OwnsOne(b => b.Dimensions, d =>
        {
            d.Property(x => x.HeightCm).HasColumnName("Dim_Height");
            d.Property(x => x.WidthCm).HasColumnName("Dim_Width");
            d.Property(x => x.DepthCm).HasColumnName("Dim_Depth");
        });
    }
}

public class EBookConfiguration : IEntityTypeConfiguration<EBook>
{
    public void Configure(EntityTypeBuilder<EBook> builder)
    {
        // Enum -> string (mais legível no banco que o valor numérico).
        builder.Property(b => b.Format).HasConversion<string>().HasMaxLength(10);
        builder.Property(b => b.DownloadUrl).HasMaxLength(500);
    }
}

public class BookAuthorConfiguration : IEntityTypeConfiguration<BookAuthor>
{
    public void Configure(EntityTypeBuilder<BookAuthor> builder)
    {
        builder.ToTable("BookAuthors");

        // Chave primária COMPOSTA (entidade de junção N:N com payload).
        builder.HasKey(ba => new { ba.BookId, ba.AuthorId });

        builder.Property(ba => ba.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(ba => ba.Book)
            .WithMany(b => b.BookAuthors)
            .HasForeignKey(ba => ba.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ba => ba.Author)
            .WithMany(a => a.BookAuthors)
            .HasForeignKey(ba => ba.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
