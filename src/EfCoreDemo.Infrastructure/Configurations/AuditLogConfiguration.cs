using EfCoreDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreDemo.Infrastructure.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        // O Id é atribuído pelo interceptor (Guid.CreateVersion7), não pelo banco.
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.EntityName).HasMaxLength(100);
        builder.Property(a => a.EntityId).HasMaxLength(60);
        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(10);
        builder.Property(a => a.User).HasMaxLength(100);

        builder.HasIndex(a => new { a.EntityName, a.EntityId });
    }
}
