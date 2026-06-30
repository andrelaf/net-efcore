namespace EfCoreDemo.Infrastructure.Common;

/// <summary>Fornece o usuário atual para preencher os campos de auditoria.</summary>
public interface ICurrentUserService
{
    string? UserName { get; }
}

/// <summary>Implementação simples; em uma API real viria do contexto HTTP/JWT.</summary>
public sealed class SystemCurrentUserService : ICurrentUserService
{
    public string? UserName { get; set; } = "system";
}

/// <summary>
/// Fonte de tempo abstraída — facilita testes determinísticos e é usada pelos
/// interceptors de auditoria. Reflete a abstração <c>TimeProvider</c> do .NET.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
