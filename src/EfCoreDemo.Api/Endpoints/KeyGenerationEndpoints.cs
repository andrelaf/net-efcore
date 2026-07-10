using EfCoreDemo.Api.Demos;
using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Infrastructure.Interceptors;
using EfCoreDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreDemo.Api.Endpoints;

public record HiLoInsertResult(
    IReadOnlyList<int> IdsAtribuidosNoAdd,
    IReadOnlyList<int> IdsDepoisDoSaveChanges,
    bool IdsConhecidosAntesDoBanco,
    string Observacao);

public static class KeyGenerationEndpoints
{
    public static void MapKeyGenerationEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/keys").WithTags("Geração de Chaves");

        g.MapPost("/hilo/insert", (AppDbContext db, SqlCaptureSink sink, int count = 3) =>
            Demo.Run(sink, "Insert com id gerado no cliente (Hi/Lo)", "UseHiLo() + SEQUENCE",
                "As categorias recebem Id JÁ no Add(), antes de qualquer INSERT. O EF reserva um bloco de ids com um único 'SELECT NEXT VALUE FOR CategoryHiLoSequence' e distribui o 'lo' em memória — repare que N inserts NÃO geram N idas à sequence. Como o Id já é conhecido, o INSERT não precisa de OUTPUT/SCOPE_IDENTITY() para lê-lo de volta. Ao final fazemos ROLLBACK: as linhas somem, mas os ids continuam consumidos — os 'buracos' são o preço conhecido do Hi/Lo.",
                async () =>
                {
                    count = Math.Clamp(count, 1, 50);

                    var categorias = Enumerable.Range(1, count)
                        .Select(i => new Category
                        {
                            Name = $"Categoria Hi/Lo {i}",
                            Slug = $"hilo-{Guid.NewGuid():N}"
                        })
                        .ToList();

                    // Add() dispara o gerador Hi/Lo. O banco só é tocado se o bloco
                    // corrente estiver esgotado (aí sai um NEXT VALUE FOR).
                    db.Categories.AddRange(categorias);
                    var idsNoAdd = categorias.Select(c => c.Id).ToList();

                    // Só agora vêm os INSERTs — e o SqlCaptureSink mostra o Id entre os parâmetros.
                    await using var tx = await db.Database.BeginTransactionAsync();
                    await db.SaveChangesAsync();
                    var idsDepois = categorias.Select(c => c.Id).ToList();
                    await tx.RollbackAsync();

                    return new HiLoInsertResult(
                        idsNoAdd,
                        idsDepois,
                        IdsConhecidosAntesDoBanco: idsNoAdd.All(id => id != 0),
                        Observacao: $"{count} insert(s) com ids já conhecidos no Add(). "
                                  + "Com IDENTITY, cada INSERT precisaria devolver o id gerado.");
                }));
    }
}
