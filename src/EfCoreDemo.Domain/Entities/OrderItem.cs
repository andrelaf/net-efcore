using EfCoreDemo.Domain.Common;
using EfCoreDemo.Domain.ValueObjects;

namespace EfCoreDemo.Domain.Entities;

/// <summary>Item de pedido — N:1 com <see cref="Order"/> e N:1 com <see cref="Book"/>.</summary>
public class OrderItem : GuidEntity
{
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    public Guid BookId { get; set; }
    public virtual Book Book { get; set; } = null!;

    public int Quantity { get; set; }
    public Money UnitPrice { get; set; } = Money.Brl(0);

    public decimal LineTotal => UnitPrice.Amount * Quantity;
}
