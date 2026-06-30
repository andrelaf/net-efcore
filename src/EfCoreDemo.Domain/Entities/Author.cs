using EfCoreDemo.Domain.Common;

namespace EfCoreDemo.Domain.Entities;

/// <summary>Autor — lado oposto do N:N com <see cref="Book"/>.</summary>
public class Author : GuidEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string Country { get; set; } = "Brasil";

    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}
