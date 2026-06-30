using EfCoreDemo.Api.Demos;
using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Infrastructure.Interceptors;
using EfCoreDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreDemo.Api.Endpoints;

public record BookTitleRow(string Title);

public static class QueryEndpoints
{
    // Consulta compilada: o plano de tradução LINQ->SQL é cacheado explicitamente.
    private static readonly Func<AppDbContext, string, IAsyncEnumerable<Book>> _booksByCategory =
        EF.CompileAsyncQuery((AppDbContext db, string slug) =>
            db.Books.Where(b => b.Category.Slug == slug));

    public static void MapQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/querying").WithTags("Consultas");

        g.MapGet("/filtered", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Global Query Filter (ativo)", "HasQueryFilter",
                "O filtro nomeado 'SoftDelete' é aplicado automaticamente: clientes com IsDeleted=1 não aparecem. Veja o WHERE injetado no SQL.",
                async () => new
                {
                    Visiveis = await db.Customers.CountAsync(),
                    Clientes = await db.Customers.Select(c => c.FullName).ToListAsync()
                }));

        g.MapGet("/ignore-filters", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Ignorar filtros", "IgnoreQueryFilters",
                "IgnoreQueryFilters() desabilita os filtros globais, revelando inclusive os registros com soft delete. No EF Core 10 é possível desabilitar um filtro pelo nome.",
                async () => new
                {
                    TodosIncluindoExcluidos = await db.Customers.IgnoreQueryFilters().CountAsync(),
                    SoSoftDeleteDesligado = await db.Customers.IgnoreQueryFilters(["SoftDelete"]).Select(c => new { c.FullName, c.IsDeleted }).ToListAsync()
                }));

        g.MapGet("/raw-sql-entities", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Raw SQL → Entidades", "FromSql (interpolado/parametrizado)",
                "FromSql executa SQL cru porém parametrizado (protege contra injeção) e ainda compõe LINQ por cima. O resultado materializa entidades rastreadas.",
                () =>
                {
                    var pattern = "%Design%";
                    return db.Books
                        .FromSql($"SELECT * FROM Books WHERE Title LIKE {pattern}")
                        .OrderBy(b => b.Title)
                        .Select(b => new { b.Title, b.Isbn })
                        .ToListAsync();
                }));

        g.MapGet("/raw-sql-scalar", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Raw SQL → Escalar", "Database.SqlQuery<T>",
                "SqlQuery<T> executa SQL arbitrário e mapeia o resultado para um tipo (aqui, um int). A coluna deve se chamar 'Value' para escalares.",
                async () => new
                {
                    TotalLivros = await db.Database
                        .SqlQuery<int>($"SELECT COUNT(*) AS Value FROM Books")
                        .FirstAsync()
                }));

        g.MapGet("/compiled-query", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Compiled Query", "EF.CompileAsyncQuery",
                "A consulta é compilada uma vez e reutilizada, evitando o custo repetido de traduzir a árvore de expressão LINQ para SQL.",
                async () =>
                {
                    var result = new List<string>();
                    await foreach (var b in _booksByCategory(db, "arquitetura"))
                        result.Add(b.Title);
                    return result;
                }));

        g.MapGet("/left-join", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "LEFT JOIN nativo (.NET 10)", "LeftJoin",
                "O operador LeftJoin do .NET 10 substitui o antigo padrão GroupJoin+SelectMany+DefaultIfEmpty. Clientes sem pedidos aparecem com pedido nulo.",
                () => db.Customers
                    .LeftJoin(
                        db.Orders,
                        c => c.Id,
                        o => o.CustomerId,
                        (c, o) => new { Cliente = c.FullName, Pedido = o == null ? "[sem pedidos]" : o.OrderNumber })
                    .ToListAsync()));
    }
}
