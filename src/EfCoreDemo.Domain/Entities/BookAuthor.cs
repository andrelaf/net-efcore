using EfCoreDemo.Domain.Enums;

namespace EfCoreDemo.Domain.Entities;

/// <summary>
/// Entidade de junção do relacionamento N:N entre <see cref="Book"/> e
/// <see cref="Author"/>, carregando dados extras (payload): o papel do autor
/// e a ordem de exibição. A chave primária é composta (BookId, AuthorId).
/// </summary>
public class BookAuthor
{
    public Guid BookId { get; set; }
    public virtual Book Book { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public virtual Author Author { get; set; } = null!;

    // Payload do relacionamento
    public AuthorRole Role { get; set; } = AuthorRole.Primary;
    public int Order { get; set; }
}
