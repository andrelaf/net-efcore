using EfCoreDemo.Domain.Common;
using EfCoreDemo.Domain.Enums;
using EfCoreDemo.Domain.ValueObjects;

namespace EfCoreDemo.Domain.Entities;

/// <summary>
/// Pedido. Demonstra: N:1 com <see cref="Customer"/>, 1:N com <see cref="OrderItem"/>,
/// 1:1 com <see cref="Payment"/> (hierarquia TPH), enum convertido para string,
/// Owned Type (<see cref="ShippingAddress"/>) e Complex Type (<see cref="Total"/>).
/// </summary>
public class Order : FullAuditedEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime PlacedAtUtc { get; set; }

    /// <summary>Enum persistido como texto via value converter.</summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public Money Total { get; set; } = Money.Brl(0);

    /// <summary>Owned Type — endereço de entrega congelado no momento do pedido.</summary>
    public Address ShippingAddress { get; set; } = new();

    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    // 1:1 com pagamento (opcional)
    public virtual Payment? Payment { get; set; }
}
