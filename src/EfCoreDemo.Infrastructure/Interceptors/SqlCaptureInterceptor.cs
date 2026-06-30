using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EfCoreDemo.Infrastructure.Interceptors;

/// <summary>
/// Acumulador (escopo por requisição) do SQL realmente executado. Permite que a
/// API devolva ao front-end o SQL gerado por cada demonstração — propósito
/// puramente didático.
/// </summary>
public sealed class SqlCaptureSink
{
    private readonly ConcurrentQueue<CapturedSql> _items = new();
    public IReadOnlyCollection<CapturedSql> Items => _items;
    public void Add(CapturedSql sql) => _items.Enqueue(sql);
    public void Clear() => _items.Clear();
}

public sealed record CapturedSql(string Sql, IReadOnlyDictionary<string, object?> Parameters, double ElapsedMs);

/// <summary>
/// <b>DbCommandInterceptor</b>: captura todo comando SQL executado (reader,
/// scalar e non-query) junto com parâmetros e tempo de execução.
/// </summary>
public sealed class SqlCaptureInterceptor(SqlCaptureSink sink) : DbCommandInterceptor
{
    private static Dictionary<string, object?> ReadParameters(DbCommand command)
    {
        var dict = new Dictionary<string, object?>();
        foreach (DbParameter p in command.Parameters)
            dict[p.ParameterName] = p.Value is null or DBNull ? null : p.Value;
        return dict;
    }

    private void Capture(DbCommand command, CommandExecutedEventData eventData)
        => sink.Add(new CapturedSql(command.CommandText, ReadParameters(command), eventData.Duration.TotalMilliseconds));

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        Capture(command, eventData);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        Capture(command, eventData);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        Capture(command, eventData);
        return base.ScalarExecuted(command, eventData, result);
    }

    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
    {
        Capture(command, eventData);
        return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        Capture(command, eventData);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Capture(command, eventData);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }
}
