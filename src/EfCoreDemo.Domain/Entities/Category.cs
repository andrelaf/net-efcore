using EfCoreDemo.Domain.Common;

namespace EfCoreDemo.Domain.Entities;

/// <summary>
/// Categoria de livros. Demonstra:
/// - PK <c>int</c> (IDENTITY) — em contraste com as PKs Guid das demais entidades;
/// - Relacionamento <b>auto-referenciado</b> (hierarquia pai/filhos).
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    // Auto-relacionamento (self-referencing)
    public int? ParentCategoryId { get; set; }
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> Children { get; set; } = new List<Category>();

    // 1:N com livros
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
