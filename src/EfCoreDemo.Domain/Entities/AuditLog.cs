using EfCoreDemo.Domain.Enums;

namespace EfCoreDemo.Domain.Entities;

/// <summary>
/// Registro de auditoria gravado automaticamente pelo
/// <c>AuditableEntityInterceptor</c> sempre que uma entidade auditável é
/// inserida, atualizada ou removida.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }

    /// <summary>JSON com os valores alterados (coluna -> {old, new}).</summary>
    public string ChangesJson { get; set; } = "{}";

    public string? User { get; set; }
    public DateTime TimestampUtc { get; set; }
}
