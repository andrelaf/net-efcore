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
        builder.HasKey(c => c.Id); // PK int IDENTITY (gerada pelo banco)

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
