using EfCoreDemo.Domain.Common;

namespace EfCoreDemo.Domain.Entities;

/// <summary>
/// Pagamento — base de uma segunda hierarquia de herança, desta vez mapeada como
/// <b>TPT</b> (Table-per-Type): cada subtipo ganha sua própria tabela ligada à
/// tabela base por FK. Contraste com a hierarquia TPH de <see cref="Book"/>.
///
/// <para>
/// Obs.: aqui o valor é um <c>decimal</c> simples, e não o Complex Type
/// <c>Money</c>. Isso é uma limitação do <b>EF Core</b> (não do banco): as colunas
/// são criadas e o INSERT funciona, mas a <i>consulta</i> falha ao montar o shaper
/// de um complex property em hierarquia TPT
/// (<c>GenerateComplexPropertyShaperExpression</c>). Verificado no EF Core 10.
/// </para>
/// </summary>
public abstract class Payment : GuidEntity
{
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BRL";
    public DateTime PaidAtUtc { get; set; }
}

public class CreditCardPayment : Payment
{
    public string CardLast4 { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int Installments { get; set; } = 1;
}

public class PixPayment : Payment
{
    public string PixKey { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}

public class BoletoPayment : Payment
{
    public string Barcode { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
}
