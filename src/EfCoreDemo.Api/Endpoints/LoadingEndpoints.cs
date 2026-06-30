using EfCoreDemo.Api.Demos;
using EfCoreDemo.Infrastructure.Interceptors;
using EfCoreDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreDemo.Api.Endpoints;

public static class LoadingEndpoints
{
    public static void MapLoadingEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/loading").WithTags("Carregamento");

        g.MapGet("/eager", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Eager Loading", "Include / ThenInclude",
                "Carrega o grafo inteiro em uma única consulta com JOINs. Repare em 1 SQL só, trazendo pedido → itens → livro.",
                () => db.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Items).ThenInclude(i => i.Book)
                    .Select(o => new
                    {
                        o.OrderNumber,
                        Cliente = o.Customer.FullName,
                        Itens = o.Items.Select(i => new { i.Book.Title, i.Quantity })
                    })
                    .ToListAsync()));

        g.MapGet("/lazy", async (AppDbContext db, SqlCaptureSink sink) =>
        {
            return await Demo.Run(sink, "Lazy Loading", "UseLazyLoadingProxies",
                "A entidade vem sem navegações. Ao ACESSAR cada navegação (Items, Customer), o proxy dispara uma consulta extra — observe múltiplos SQL (N+1).",
                async () =>
                {
                    var order = await db.Orders.FirstAsync();   // 1 SQL
                    var itensCount = order.Items.Count;          // +1 SQL (lazy)
                    var cliente = order.Customer.FullName;       // +1 SQL (lazy)
                    return new { order.OrderNumber, itensCount, cliente };
                });
        });

        g.MapGet("/explicit", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Explicit Loading", "Entry().Collection()/Reference().Load()",
                "Carrega a entidade primeiro e, sob demanda, pede explicitamente cada navegação. Útil para decidir em runtime o que carregar.",
                async () =>
                {
                    var customer = await db.Customers.FirstAsync();
                    await db.Entry(customer).Reference(c => c.Profile).LoadAsync();
                    await db.Entry(customer).Collection(c => c.Orders).LoadAsync();
                    return new
                    {
                        customer.FullName,
                        TemPerfil = customer.Profile != null,
                        Pedidos = customer.Orders.Count
                    };
                }));

        g.MapGet("/split-query", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Split Query", "AsSplitQuery",
                "Evita a explosão cartesiana de múltiplos Includes de coleção: o EF emite uma consulta por coleção em vez de um JOIN gigante.",
                () => db.Orders
                    .Include(o => o.Items).ThenInclude(i => i.Book)
                    .AsSplitQuery()
                    .Select(o => new
                    {
                        o.OrderNumber,
                        Itens = o.Items.Select(i => new { i.Book.Title, i.Quantity })
                    })
                    .ToListAsync()));

        g.MapGet("/projection", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Projeção para DTO", "Select",
                "Em vez de materializar entidades inteiras, projeta só as colunas necessárias — SQL mais enxuto e sem tracking.",
                () => db.Books
                    .Select(b => new { b.Title, Preco = b.Price.Amount, b.Isbn })
                    .ToListAsync()));

        g.MapGet("/no-tracking", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "No-Tracking", "AsNoTracking",
                "Consultas somente-leitura não precisam do change tracker. AsNoTracking reduz memória e CPU.",
                () => db.Books
                    .AsNoTracking()
                    .Select(b => new { b.Title })
                    .ToListAsync()));
    }
}
