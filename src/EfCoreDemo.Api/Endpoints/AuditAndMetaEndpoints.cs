using EfCoreDemo.Api.Demos;
using EfCoreDemo.Infrastructure.Interceptors;
using EfCoreDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreDemo.Api.Endpoints;

public record DemoInfo(string Group, string Title, string Technique, string Method, string Path);

public static class AuditAndMetaEndpoints
{
    public static void MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/audit").WithTags("Auditoria");

        g.MapGet("/logs", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Trilha de auditoria", "SaveChangesInterceptor",
                "O AuditableEntityInterceptor grava um AuditLog (com diff em JSON) a cada insert/update/delete de entidade auditável. Veja os registros mais recentes.",
                () => db.AuditLogs
                    .OrderByDescending(a => a.TimestampUtc)
                    .Take(25)
                    .Select(a => new
                    {
                        a.EntityName,
                        a.EntityId,
                        Acao = a.Action.ToString(),
                        a.User,
                        a.TimestampUtc,
                        a.ChangesJson
                    })
                    .ToListAsync()));
    }

    public static void MapMetaEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/demos", () => Results.Ok(Catalog)).WithTags("Catálogo");
    }

    /// <summary>Catálogo de todas as demonstrações — consumido pelo front React.</summary>
    public static readonly DemoInfo[] Catalog =
    [
        new("Relacionamentos", "1:1 — Cliente e Perfil", "One-to-One", "GET", "/api/relationships/one-to-one"),
        new("Relacionamentos", "1:N — Pedido e Itens", "One-to-Many", "GET", "/api/relationships/one-to-many"),
        new("Relacionamentos", "N:N com payload", "Many-to-Many", "GET", "/api/relationships/many-to-many"),
        new("Relacionamentos", "Auto-relacionamento", "Self-Referencing", "GET", "/api/relationships/self-referencing"),
        new("Relacionamentos", "Herança TPH", "Table-per-Hierarchy", "GET", "/api/relationships/inheritance-tph"),
        new("Relacionamentos", "Herança TPT", "Table-per-Type", "GET", "/api/relationships/inheritance-tpt"),

        new("Carregamento", "Eager Loading", "Include/ThenInclude", "GET", "/api/loading/eager"),
        new("Carregamento", "Lazy Loading", "Proxies", "GET", "/api/loading/lazy"),
        new("Carregamento", "Explicit Loading", "Entry().Load()", "GET", "/api/loading/explicit"),
        new("Carregamento", "Split Query", "AsSplitQuery", "GET", "/api/loading/split-query"),
        new("Carregamento", "Projeção DTO", "Select", "GET", "/api/loading/projection"),
        new("Carregamento", "No-Tracking", "AsNoTracking", "GET", "/api/loading/no-tracking"),

        new("Consultas", "Filtro global ativo", "HasQueryFilter", "GET", "/api/querying/filtered"),
        new("Consultas", "Ignorar filtros", "IgnoreQueryFilters", "GET", "/api/querying/ignore-filters"),
        new("Consultas", "Raw SQL → Entidades", "FromSql", "GET", "/api/querying/raw-sql-entities"),
        new("Consultas", "Raw SQL → Escalar", "SqlQuery<T>", "GET", "/api/querying/raw-sql-scalar"),
        new("Consultas", "Compiled Query", "EF.CompileAsyncQuery", "GET", "/api/querying/compiled-query"),
        new("Consultas", "LEFT JOIN (.NET 10)", "LeftJoin", "GET", "/api/querying/left-join"),

        new("Modificações", "Update em lote", "ExecuteUpdateAsync", "POST", "/api/mutations/batch-update"),
        new("Modificações", "Delete em lote", "ExecuteDeleteAsync", "POST", "/api/mutations/batch-delete"),
        new("Modificações", "Transação (rollback)", "BeginTransaction", "POST", "/api/mutations/transaction"),
        new("Modificações", "Concorrência otimista", "rowversion", "POST", "/api/mutations/concurrency"),

        new("Geração de Chaves", "Hi/Lo (id gerado no cliente)", "UseHiLo", "POST", "/api/keys/hilo/insert"),

        new("Auditoria", "Trilha de auditoria", "SaveChangesInterceptor", "GET", "/api/audit/logs"),
    ];
}
