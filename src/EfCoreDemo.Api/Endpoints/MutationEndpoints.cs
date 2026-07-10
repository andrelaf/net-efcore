using EfCoreDemo.Api.Demos;
using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Domain.Enums;
using EfCoreDemo.Domain.ValueObjects;
using EfCoreDemo.Infrastructure.Interceptors;
using EfCoreDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreDemo.Api.Endpoints;

public record ConcurrencyResult(bool ConflitoDetectado, string RowVersionOriginal, string Mensagem);

public static class MutationEndpoints
{
    public static void MapMutationEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/mutations").WithTags("Modificações");

        g.MapPost("/batch-update", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Update em lote (atômico)", "ExecuteUpdateAsync",
                "Atualiza N linhas com um único UPDATE no banco, SEM carregar entidades nem usar o change tracker. Atenção: por isso NÃO dispara o interceptor de auditoria.",
                async () =>
                {
                    var afetados = await db.Customers
                        .ExecuteUpdateAsync(s => s.SetProperty(c => c.LoyaltyPoints, c => c.LoyaltyPoints + 10));
                    return new { LinhasAfetadas = afetados };
                }));

        g.MapPost("/batch-delete", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Delete em lote (atômico)", "ExecuteDeleteAsync",
                "Remove linhas com um único DELETE, sem materializar entidades. Aqui limpamos logs de auditoria com mais de 7 dias.",
                async () =>
                {
                    var cutoff = DateTime.UtcNow.AddDays(-7);
                    var removidos = await db.AuditLogs
                        .Where(a => a.TimestampUtc < cutoff)
                        .ExecuteDeleteAsync();
                    return new { LinhasRemovidas = removidos };
                }));

        g.MapPost("/transaction", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Transação explícita (rollback)", "BeginTransaction / Rollback",
                "Demonstra atomicidade: inserimos um cliente dentro de uma transação, confirmamos a contagem 'durante' e fazemos ROLLBACK — o estado final volta ao original.",
                async () =>
                {
                    await using var tx = await db.Database.BeginTransactionAsync();
                    var antes = await db.Customers.IgnoreQueryFilters().CountAsync();

                    db.Customers.Add(new Customer
                    {
                        FullName = "Cliente Temporário",
                        Email = $"temp-{Guid.NewGuid():N}@example.com",
                        Address = new Address { City = "Tmp", State = "SP" }
                    });
                    await db.SaveChangesAsync();
                    var durante = await db.Customers.IgnoreQueryFilters().CountAsync();

                    await tx.RollbackAsync();
                    var depois = await db.Customers.IgnoreQueryFilters().CountAsync();

                    return new { antes, durante, depois };
                }));

        g.MapPost("/concurrency", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Conflito de concorrência otimista", "rowversion / DbUpdateConcurrencyException",
                "Carregamos um pedido e, antes de salvar, simulamos OUTRO usuário dando um UPDATE direto na linha — o SQL Server incrementa o rowversion sozinho. O UPDATE do EF leva o rowversion ORIGINAL no WHERE, casa 0 linhas e lança DbUpdateConcurrencyException.",
                async () =>
                {
                    var order = await db.Orders.FirstAsync();
                    var versaoOriginal = Convert.ToBase64String(order.RowVersion);
                    order.Status = order.Status == OrderStatus.Pending ? OrderStatus.Paid : OrderStatus.Pending;

                    // Outro usuário tocou a linha entre o load e o save. Qualquer UPDATE
                    // basta: o banco reescreve o rowversion por conta própria.
                    await db.Database.ExecuteSqlInterpolatedAsync(
                        $"UPDATE Orders SET UpdatedAtUtc = SYSUTCDATETIME() WHERE Id = {order.Id}");

                    try
                    {
                        await db.SaveChangesAsync();
                        return new ConcurrencyResult(false, versaoOriginal, "Salvou sem conflito (inesperado).");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return new ConcurrencyResult(
                            true,
                            versaoOriginal,
                            "DbUpdateConcurrencyException capturada: a linha foi modificada por outro processo. Em produção: recarregar, mesclar e tentar de novo.");
                    }
                }));
    }
}
