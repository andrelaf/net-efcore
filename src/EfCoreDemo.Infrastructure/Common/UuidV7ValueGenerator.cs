using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace EfCoreDemo.Infrastructure.Common;

/// <summary>
/// Gera chaves <see cref="Guid"/> no formato <b>UUID v7</b> (ordenável por tempo),
/// disponível nativamente no .NET via <see cref="Guid.CreateVersion7()"/>.
/// UUID v7 melhora a localidade de índice em relação ao Guid aleatório (v4),
/// reduzindo fragmentação de páginas em inserts.
/// </summary>
public sealed class UuidV7ValueGenerator : ValueGenerator<Guid>
{
    /// <summary>Valores são gerados pela aplicação, não pelo banco.</summary>
    public override bool GeneratesTemporaryValues => false;

    public override Guid Next(EntityEntry entry) => Guid.CreateVersion7();
}
