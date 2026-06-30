using EfCoreDemo.Domain.Common;
using EfCoreDemo.Domain.ValueObjects;

namespace EfCoreDemo.Domain.Entities;

/// <summary>
/// Cliente. Demonstra: PK Guid (UUID v7), Complex Type (<see cref="Address"/>),
/// relacionamento 1:1 com <see cref="CustomerProfile"/>, 1:N com <see cref="Order"/>,
/// auditoria, soft delete e concorrência otimista.
/// As navegações são <c>virtual</c> para habilitar Lazy Loading via proxies.
/// </summary>
public class Customer : FullAuditedEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }

    /// <summary>Complex Type — mapeado em colunas da tabela Customers.</summary>
    public Address Address { get; set; } = new();

    // 1:1 (principal) — a FK fica em CustomerProfile.
    public virtual CustomerProfile? Profile { get; set; }

    // 1:N
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
