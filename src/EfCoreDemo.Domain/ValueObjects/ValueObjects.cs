namespace EfCoreDemo.Domain.ValueObjects;

/// <summary>
/// Endereço usado como <b>Complex Type</b> (EF Core). Diferente de um Owned Type,
/// um complex type não tem identidade/chave própria e é sempre parte da entidade.
/// Mapeado em colunas da própria tabela do dono.
/// </summary>
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Brasil";
}

/// <summary>
/// Valor monetário usado como Complex Type (Amount + Currency).
/// </summary>
public class Money
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BRL";

    public static Money Brl(decimal amount) => new() { Amount = amount, Currency = "BRL" };

    public override string ToString() => $"{Currency} {Amount:N2}";
}

/// <summary>
/// Metadados de um livro. Mapeado como Complex Type serializado em coluna JSON
/// (recurso do EF Core 10: <c>ComplexProperty(...).ToJson()</c>).
/// </summary>
public class BookMetadata
{
    public string Publisher { get; set; } = string.Empty;
    public int Edition { get; set; } = 1;
    public int? PageCount { get; set; }
    public string Language { get; set; } = "pt-BR";
}

/// <summary>
/// Dimensões físicas, usadas dentro de um <b>Owned Type</b> em PhysicalBook.
/// </summary>
public class Dimensions
{
    public double HeightCm { get; set; }
    public double WidthCm { get; set; }
    public double DepthCm { get; set; }
}
