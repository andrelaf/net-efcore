using EfCoreDemo.Domain.Entities;
using EfCoreDemo.Domain.Enums;
using EfCoreDemo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EfCoreDemo.Infrastructure.Persistence;

/// <summary>Popula o banco com um conjunto de dados de demonstração.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Categories.AnyAsync()) return; // já populado

        // ---- Categorias (hierarquia auto-referenciada) ----
        var tech = new Category { Name = "Tecnologia", Slug = "tecnologia" };
        var prog = new Category { Name = "Programação", Slug = "programacao", ParentCategory = tech };
        var arch = new Category { Name = "Arquitetura de Software", Slug = "arquitetura", ParentCategory = tech };
        var fic = new Category { Name = "Ficção", Slug = "ficcao" };

        // ---- Autores ----
        var martin = new Author { Name = "Robert C. Martin", Country = "EUA", Bio = "Uncle Bob" };
        var fowler = new Author { Name = "Martin Fowler", Country = "EUA", Bio = "Refactoring, P of EAA" };
        var evans = new Author { Name = "Eric Evans", Country = "EUA", Bio = "Domain-Driven Design" };
        var tolkien = new Author { Name = "J. R. R. Tolkien", Country = "Reino Unido" };

        // ---- Livros (TPH: físicos e ebooks) ----
        var cleanCode = new PhysicalBook
        {
            Title = "Clean Code",
            Isbn = "9780132350884",
            Price = Money.Brl(189.90m),
            CategoryId = 0,
            Category = prog,
            StockQuantity = 25,
            WeightGrams = 700,
            Dimensions = new Dimensions { HeightCm = 23, WidthCm = 17, DepthCm = 3 },
            Tags = ["clean-code", "boas-praticas", "oop"],
            Metadata = new BookMetadata { Publisher = "Prentice Hall", Edition = 1, PageCount = 464, Language = "en" }
        };
        cleanCode.BookAuthors.Add(new BookAuthor { Author = martin, Role = AuthorRole.Primary, Order = 1 });

        var ddd = new PhysicalBook
        {
            Title = "Domain-Driven Design",
            Isbn = "9780321125217",
            Price = Money.Brl(259.90m),
            Category = arch,
            StockQuantity = 12,
            WeightGrams = 950,
            Dimensions = new Dimensions { HeightCm = 24, WidthCm = 18, DepthCm = 4 },
            Tags = ["ddd", "modelagem", "arquitetura"],
            Metadata = new BookMetadata { Publisher = "Addison-Wesley", Edition = 1, PageCount = 560, Language = "en" }
        };
        ddd.BookAuthors.Add(new BookAuthor { Author = evans, Role = AuthorRole.Primary, Order = 1 });

        var refactoring = new EBook
        {
            Title = "Refactoring",
            Isbn = "9780134757599",
            Price = Money.Brl(149.90m),
            Category = arch,
            Format = EbookFormat.Epub,
            FileSizeMb = 8.5,
            DownloadUrl = "https://example.com/refactoring.epub",
            Tags = ["refactoring", "design"],
            Metadata = new BookMetadata { Publisher = "Addison-Wesley", Edition = 2, PageCount = 448, Language = "en" }
        };
        refactoring.BookAuthors.Add(new BookAuthor { Author = fowler, Role = AuthorRole.Primary, Order = 1 });

        var hobbit = new PhysicalBook
        {
            Title = "O Hobbit",
            Isbn = "9788595084742",
            Price = Money.Brl(54.90m),
            Category = fic,
            StockQuantity = 100,
            WeightGrams = 400,
            Dimensions = new Dimensions { HeightCm = 21, WidthCm = 14, DepthCm = 2 },
            Tags = ["fantasia", "aventura"],
            Metadata = new BookMetadata { Publisher = "HarperCollins", Edition = 7, PageCount = 336, Language = "pt-BR" }
        };
        hobbit.BookAuthors.Add(new BookAuthor { Author = tolkien, Role = AuthorRole.Primary, Order = 1 });

        // ---- Clientes (1:1 com perfil + complex type Address) ----
        var ana = new Customer
        {
            FullName = "Ana Souza",
            Email = "ana.souza@example.com",
            LoyaltyPoints = 120,
            Address = new Address { Street = "Rua das Flores", Number = "100", City = "São Paulo", State = "SP", ZipCode = "01000-000" },
            Profile = new CustomerProfile
            {
                Bio = "Desenvolvedora back-end",
                PreferredLanguage = "pt-BR",
                Birthday = new DateOnly(1990, 5, 12),
                Interests = ["dotnet", "ddd", "ef-core"]
            }
        };

        var bruno = new Customer
        {
            FullName = "Bruno Lima",
            Email = "bruno.lima@example.com",
            LoyaltyPoints = 40,
            Address = new Address { Street = "Av. Central", Number = "2500", City = "Curitiba", State = "PR", ZipCode = "80000-000" },
            Profile = new CustomerProfile
            {
                Bio = "Arquiteto de soluções",
                Interests = ["arquitetura", "cloud"]
            }
        };

        // Cliente SEM pedidos — útil para a demo de LEFT JOIN.
        var carla = new Customer
        {
            FullName = "Carla Dias",
            Email = "carla.dias@example.com",
            LoyaltyPoints = 0,
            Address = new Address { Street = "Rua Nova", Number = "5", City = "Recife", State = "PE", ZipCode = "50000-000" }
        };

        // Cliente com SOFT DELETE — escondido pelo Global Query Filter 'SoftDelete'.
        var deletado = new Customer
        {
            FullName = "Cliente Excluído",
            Email = "excluido@example.com",
            Address = new Address { City = "—", State = "SP" },
            IsDeleted = true,
            DeletedAtUtc = DateTime.UtcNow.AddDays(-10)
        };

        // ---- Pedido com itens, owned shipping address e pagamento (TPT) ----
        var order1 = new Order
        {
            OrderNumber = "ORD-0001",
            PlacedAtUtc = DateTime.UtcNow.AddDays(-3),
            Status = OrderStatus.Paid,
            Customer = ana,
            ShippingAddress = new Address { Street = "Rua das Flores", Number = "100", City = "São Paulo", State = "SP", ZipCode = "01000-000" },
            Total = Money.Brl(189.90m + 259.90m)
        };
        order1.Items.Add(new OrderItem { Book = cleanCode, Quantity = 1, UnitPrice = Money.Brl(189.90m) });
        order1.Items.Add(new OrderItem { Book = ddd, Quantity = 1, UnitPrice = Money.Brl(259.90m) });
        order1.Payment = new CreditCardPayment
        {
            Amount = 449.80m,
            PaidAtUtc = DateTime.UtcNow.AddDays(-3),
            Brand = "Visa",
            CardLast4 = "4242",
            Installments = 3
        };

        var order2 = new Order
        {
            OrderNumber = "ORD-0002",
            PlacedAtUtc = DateTime.UtcNow.AddDays(-1),
            Status = OrderStatus.Pending,
            Customer = bruno,
            ShippingAddress = new Address { Street = "Av. Central", Number = "2500", City = "Curitiba", State = "PR", ZipCode = "80000-000" },
            Total = Money.Brl(149.90m)
        };
        order2.Items.Add(new OrderItem { Book = refactoring, Quantity = 1, UnitPrice = Money.Brl(149.90m) });
        order2.Payment = new PixPayment
        {
            Amount = 149.90m,
            PaidAtUtc = DateTime.UtcNow.AddHours(-20),
            PixKey = "bruno.lima@example.com",
            TransactionId = "PIX-ABC-123"
        };

        db.AddRange(tech, prog, arch, fic);
        db.AddRange(cleanCode, ddd, refactoring, hobbit);
        db.AddRange(ana, bruno, carla, deletado);
        db.AddRange(order1, order2);

        await db.SaveChangesAsync();
    }
}
