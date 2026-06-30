using EfCoreDemo.Infrastructure.Interceptors;

namespace EfCoreDemo.Api.Demos;

/// <summary>
/// Envelope retornado por toda demonstração: além do dado, devolve o SQL
/// realmente executado e uma explicação didática da técnica.
/// </summary>
public record DemoResult(
    string Title,
    string Technique,
    string Explanation,
    object? Data,
    IReadOnlyCollection<CapturedSql> Sql,
    int SqlCount);

public static class Demo
{
    /// <summary>
    /// Limpa o sink, executa a ação (capturando o SQL gerado nela) e empacota a
    /// resposta. Como o <see cref="SqlCaptureSink"/> é escopo-por-requisição,
    /// cada chamada começa sem ruído de SQL anterior.
    /// </summary>
    public static async Task<IResult> Run<T>(
        SqlCaptureSink sink,
        string title,
        string technique,
        string explanation,
        Func<Task<T>> action)
    {
        sink.Clear();
        var data = await action();
        var sql = sink.Items.ToList();
        return Results.Ok(new DemoResult(title, technique, explanation, data, sql, sql.Count));
    }
}
