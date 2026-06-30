using EfCoreDemo.Api.Demos;
using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Infrastructure.Interceptors;
using EfCoreDemo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EfCoreDemo.Api.Endpoints;

public static class RelationshipEndpoints
{
    public static void MapRelationshipEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/relationships").WithTags("Relacionamentos");

        g.MapGet("/one-to-one", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "1:1 — Cliente e Perfil", "One-to-One",
                "Customer (principal) e CustomerProfile (dependente) compartilham a mesma chave lógica via FK única. Include traz o perfil junto.",
                () => db.Customers
                    .Include(c => c.Profile)
                    .Select(c => new
                    {
                        c.Id,
                        c.FullName,
                        c.Email,
                        Perfil = c.Profile == null ? null : new
                        {
                            c.Profile.Bio,
                            c.Profile.PreferredLanguage,
                            c.Profile.Interests
                        }
                    })
                    .ToListAsync()));

        g.MapGet("/one-to-many", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "1:N — Pedido e Itens", "One-to-Many",
                "Um Order possui vários OrderItem. A FK OrderId fica no lado 'muitos'.",
                () => db.Orders
                    .Include(o => o.Items)
                    .Select(o => new
                    {
                        o.OrderNumber,
                        Status = o.Status.ToString(),
                        Total = o.Total.Amount,
                        Itens = o.Items.Select(i => new { i.Quantity, Preco = i.UnitPrice.Amount })
                    })
                    .ToListAsync()));

        g.MapGet("/many-to-many", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "N:N com payload — Livro e Autores", "Many-to-Many (join entity)",
                "Book e Author se relacionam via BookAuthor, que carrega dados extras (Role, Order). PK composta (BookId, AuthorId).",
                () => db.Books
                    .Select(b => new
                    {
                        b.Title,
                        Autores = b.BookAuthors
                            .OrderBy(ba => ba.Order)
                            .Select(ba => new { ba.Author.Name, Papel = ba.Role.ToString() })
                    })
                    .ToListAsync()));

        g.MapGet("/self-referencing", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Auto-relacionamento — Hierarquia de Categorias", "Self-Referencing",
                "Category tem ParentCategoryId apontando para a própria tabela, formando uma árvore.",
                () => db.Categories
                    .Where(c => c.ParentCategoryId == null)
                    .Include(c => c.Children)
                    .Select(c => new
                    {
                        c.Name,
                        Subcategorias = c.Children.Select(ch => ch.Name)
                    })
                    .ToListAsync()));

        g.MapGet("/inheritance-tph", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Herança TPH — Livros", "Table-per-Hierarchy",
                "PhysicalBook e EBook ficam na mesma tabela 'Books', distinguidos pela coluna discriminadora 'BookType'. OfType<T>() filtra por subtipo.",
                async () => new
                {
                    Fisicos = await db.Books.OfType<PhysicalBook>()
                        .Select(b => new { b.Title, b.StockQuantity, b.WeightGrams }).ToListAsync(),
                    Digitais = await db.Books.OfType<EBook>()
                        .Select(b => new { b.Title, Formato = b.Format.ToString(), b.FileSizeMb }).ToListAsync()
                }));

        g.MapGet("/inheritance-tpt", (AppDbContext db, SqlCaptureSink sink) =>
            Demo.Run(sink, "Herança TPT — Pagamentos", "Table-per-Type",
                "Payment é a tabela base; cada subtipo (CreditCard, Pix, Boleto) tem sua própria tabela ligada por FK. EF faz JOIN (LEFT JOIN nas tabelas dos subtipos) para materializar a hierarquia.",
                async () =>
                {
                    var pagamentos = await db.Payments.ToListAsync();
                    return pagamentos.Select(p => new
                    {
                        Tipo = p switch
                        {
                            CreditCardPayment => "Cartão de Crédito",
                            PixPayment => "Pix",
                            BoletoPayment => "Boleto",
                            _ => "?"
                        },
                        Valor = p.Amount,
                        p.PaidAtUtc,
                        Detalhe = p switch
                        {
                            CreditCardPayment cc => $"{cc.Brand} final {cc.CardLast4} em {cc.Installments}x",
                            PixPayment px => $"Chave: {px.PixKey}",
                            BoletoPayment bo => $"Código: {bo.Barcode}",
                            _ => ""
                        }
                    });
                }));
    }
}
