using EfCoreDemo.Domain.Common;
using EfCoreDemo.Domain.Enums;
using EfCoreDemo.Domain.ValueObjects;

namespace EfCoreDemo.Domain.Entities;

/// <summary>
/// Livro — classe base de uma hierarquia <b>TPH</b> (Table-per-Hierarchy).
/// As subclasses <see cref="PhysicalBook"/> e <see cref="EBook"/> compartilham a
/// mesma tabela, distinguidas por uma coluna discriminadora.
/// Demonstra também: Complex Type em JSON (<see cref="Metadata"/>),
/// coleção primitiva (<see cref="Tags"/>) e N:N com payload (<see cref="BookAuthor"/>).
/// </summary>
public abstract class Book : FullAuditedEntity
{
    public string Title { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;

    /// <summary>Preço como Complex Type (Money).</summary>
    public Money Price { get; set; } = Money.Brl(0);

    /// <summary>Metadados gravados como JSON em uma única coluna.</summary>
    public BookMetadata Metadata { get; set; } = new();

    /// <summary>Tags livres — coleção primitiva (JSON).</summary>
    public List<string> Tags { get; set; } = new();

    // N:1
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    // N:N com payload (via entidade de junção BookAuthor)
    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

/// <summary>Livro físico — possui estoque e dimensões (Owned Type).</summary>
public class PhysicalBook : Book
{
    public int StockQuantity { get; set; }
    public double WeightGrams { get; set; }

    /// <summary>Owned Type — mapeado em colunas com prefixo na própria tabela.</summary>
    public Dimensions Dimensions { get; set; } = new();
}

/// <summary>Livro digital — sem estoque, com formato e tamanho de arquivo.</summary>
public class EBook : Book
{
    public EbookFormat Format { get; set; }
    public double FileSizeMb { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}
