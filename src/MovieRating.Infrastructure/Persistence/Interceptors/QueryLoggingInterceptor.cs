using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;

namespace MovieRating.Infrastructure.Persistence.Interceptors;

public class QueryLoggingInterceptor : DbCommandInterceptor
{
    private readonly ILogger<QueryLoggingInterceptor> _logger;
    private readonly Dictionary<Guid, Stopwatch> _commandTimings;

    public QueryLoggingInterceptor(ILogger<QueryLoggingInterceptor> logger)
    {
        _logger = logger;
        _commandTimings = new Dictionary<Guid, Stopwatch>();
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Activity.Current?.Id ?? eventData.CommandId.ToString();
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _commandTimings[eventData.CommandId] = stopwatch;

        _logger.LogInformation(
            "Executing query for CorrelationId: {CorrelationId}\nSQL: {Sql}\nParameters: {Parameters}",
            correlationId,
            command.CommandText,
            command.Parameters.Cast<DbParameter>()
                .Select(p => $"{p.ParameterName} = {p.Value}")
                .ToList());

        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        if (_commandTimings.TryGetValue(eventData.CommandId, out var stopwatch))
        {
            stopwatch.Stop();
            var correlationId = Activity.Current?.Id ?? eventData.CommandId.ToString();
            var duration = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Query completed for CorrelationId: {CorrelationId} in {Duration}ms",
                correlationId,
                duration);

            _commandTimings.Remove(eventData.CommandId);
        }

        return base.ReaderExecuted(command, eventData, result);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        if (_commandTimings.TryGetValue(eventData.CommandId, out var stopwatch))
        {
            stopwatch.Stop();
            var correlationId = Activity.Current?.Id ?? eventData.CommandId.ToString();
            var duration = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Query completed for CorrelationId: {CorrelationId} in {Duration}ms",
                correlationId,
                duration);

            _commandTimings.Remove(eventData.CommandId);
        }

        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        if (_commandTimings.TryGetValue(eventData.CommandId, out var stopwatch))
        {
            stopwatch.Stop();
            _commandTimings.Remove(eventData.CommandId);
        }

        _logger.LogError(
            eventData.Exception,
            "Query failed for CommandId: {CommandId}\nSQL: {Sql}",
            eventData.CommandId,
            command.CommandText);

        base.CommandFailed(command, eventData);
    }
}