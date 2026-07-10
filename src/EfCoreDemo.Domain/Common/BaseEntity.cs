namespace EfCoreDemo.Domain.Common;

/// <summary>
/// Base para entidades cuja chave primária é um <see cref="Guid"/> gerado como
/// UUID v7 (ordenável por tempo). A geração efetiva acontece via
/// <c>UuidV7ValueGenerator</c> configurado no mapeamento.
/// </summary>
public abstract class GuidEntity
{
    public Guid Id { get; set; }
}

/// <summary>
/// Base que já agrega auditoria + soft delete + concorrência otimista para
/// reduzir repetição nas entidades de domínio.
/// </summary>
public abstract class FullAuditedEntity : GuidEntity, IAuditable, ISoftDeletable, IConcurrencyAware
{
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
