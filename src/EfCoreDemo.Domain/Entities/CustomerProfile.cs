using EfCoreDemo.Domain.Common;

namespace EfCoreDemo.Domain.Entities;

/// <summary>
/// Perfil do cliente — lado dependente do relacionamento 1:1.
/// Demonstra ainda uma <b>coleção primitiva</b> (<see cref="Interests"/>) que o
/// EF Core 10 mapeia para uma coluna JSON automaticamente.
/// </summary>
public class CustomerProfile : GuidEntity
{
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public string? Bio { get; set; }
    public DateOnly? Birthday { get; set; }
    public string PreferredLanguage { get; set; } = "pt-BR";

    /// <summary>Coleção primitiva mapeada para JSON pelo EF Core.</summary>
    public List<string> Interests { get; set; } = new();
}
