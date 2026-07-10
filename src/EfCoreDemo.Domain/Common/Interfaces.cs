namespace EfCoreDemo.Domain.Common;

/// <summary>
/// Entidade auditável. Os campos são preenchidos automaticamente pelo
/// <c>AuditableEntityInterceptor</c> (um <c>SaveChangesInterceptor</c>).
/// </summary>
public interface IAuditable
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}

/// <summary>
/// Entidade com exclusão lógica (soft delete). Combinado com um
/// Global Query Filter, registros marcados como excluídos somem das consultas.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
}

/// <summary>
/// Entidade com controle de concorrência otimista, apoiada no tipo
/// <c>rowversion</c> do SQL Server: o próprio banco incrementa a coluna a cada
/// UPDATE, e o EF a inclui no <c>WHERE</c>. Nada de token mantido à mão.
///
/// <para>
/// <b>Quando usar concorrência otimista:</b> quando vários usuários/processos
/// podem editar o MESMO registro e a colisão é rara, mas sobrescrever dados
/// silenciosamente seria inaceitável (ex.: editar um pedido, ajustar estoque,
/// alterar um cadastro compartilhado). Em vez de travar a linha (pessimista),
/// deixa-se a edição acontecer e valida-se no momento do save: se o token mudou
/// desde a leitura, o EF lança <c>DbUpdateConcurrencyException</c> e a aplicação
/// decide a estratégia (recarregar e reaplicar, mesclar campos, ou avisar o
/// usuário — "client wins" vs "store wins").
/// </para>
/// <para>
/// <b>Quando NÃO compensa:</b> em escritas de alta contenção sobre a mesma linha
/// (muitos conflitos ⇒ muitos retries) ou quando a operação já é atômica no banco
/// (ex.: <c>ExecuteUpdate</c> com cálculo no próprio SQL), onde o token sequer é
/// avaliado pelo change tracker.
/// </para>
/// </summary>
public interface IConcurrencyAware
{
    /// <summary>Coluna <c>rowversion</c>: gerada e mantida pelo banco.</summary>
    byte[] RowVersion { get; set; }
}
