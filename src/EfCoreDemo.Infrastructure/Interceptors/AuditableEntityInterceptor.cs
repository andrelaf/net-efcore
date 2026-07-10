using System.Text.Json;
using EfCoreDemo.Domain.Common;
using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Domain.Enums;
using EfCoreDemo.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfCoreDemo.Infrastructure.Interceptors;

/// <summary>
/// <b>SaveChangesInterceptor</b> que centraliza regras transversais antes de
/// persistir:
/// <list type="bullet">
/// <item>Preenche campos de auditoria (<see cref="IAuditable"/>);</item>
/// <item>Converte DELETE físico em soft delete (<see cref="ISoftDeletable"/>);</item>
/// <item>Não toca em <see cref="IConcurrencyAware"/>: o <c>rowversion</c> é do banco;</item>
/// <item>Gera registros de <see cref="AuditLog"/> com o diff das alterações.</item>
/// </list>
/// </summary>
public sealed class AuditableEntityInterceptor(ICurrentUserService currentUser, IClock clock) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null) Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null) Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext context)
    {
        var now = clock.UtcNow;
        var user = currentUser.UserName;
        var auditLogs = new List<AuditLog>();

        // ChangeTracker.Entries() é avaliado uma vez; alterar estados aqui é seguro.
        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            if (entry.Entity is AuditLog) continue; // evita auditar a própria auditoria

            // 1) Soft delete: intercepta a remoção e a transforma em update.
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable soft)
            {
                entry.State = EntityState.Modified;
                soft.IsDeleted = true;
                soft.DeletedAtUtc = now;
            }

            // 2) Auditoria de timestamps/usuário.
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAtUtc = now;
                    auditable.CreatedBy = user;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAtUtc = now;
                    auditable.UpdatedBy = user;
                }
            }

            // 3) Concorrência otimista: nada a fazer. A coluna 'rowversion' do
            //    SQL Server é incrementada pelo próprio banco a cada UPDATE.

            // 4) Trilha de auditoria (somente entidades auditáveis).
            if (entry.Entity is IAuditable && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                auditLogs.Add(BuildAuditLog(entry, now, user));
            }
        }

        if (auditLogs.Count > 0)
            context.Set<AuditLog>().AddRange(auditLogs);
    }

    private static AuditLog BuildAuditLog(EntityEntry entry, DateTime now, string? user)
    {
        var action = entry.State switch
        {
            EntityState.Added => AuditAction.Insert,
            EntityState.Deleted => AuditAction.Delete,
            _ => AuditAction.Update
        };

        var changes = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (action == AuditAction.Update && !prop.IsModified) continue;
            changes[prop.Metadata.Name] = action switch
            {
                AuditAction.Insert => prop.CurrentValue,
                AuditAction.Delete => prop.OriginalValue,
                _ => new { old = prop.OriginalValue, @new = prop.CurrentValue }
            };
        }

        var keyValue = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue;

        return new AuditLog
        {
            Id = Guid.CreateVersion7(),
            EntityName = entry.Entity.GetType().Name,
            EntityId = keyValue?.ToString() ?? "?",
            Action = action,
            ChangesJson = JsonSerializer.Serialize(changes),
            User = user,
            TimestampUtc = now
        };
    }
}
