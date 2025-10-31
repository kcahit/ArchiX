// File: src/ArchiX.Library/Infrastructure/EFCore/DbCommandMetricsInterceptor.cs
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ArchiX.Library.Infrastructure.EfCore
{
    /// <summary>
    /// EF Core DbCommand metrikleri.
    /// </summary>
    public sealed class DbCommandMetricsInterceptor : DbCommandInterceptor
    {
        private readonly Counter<long> _total;
        private readonly Counter<long> _failed;
        private readonly Histogram<double> _durationMs;
        private readonly ConcurrentDictionary<Guid, long> _startTicks = new();

        public DbCommandMetricsInterceptor(Meter meter)
        {
            ArgumentNullException.ThrowIfNull(meter);
            _total = meter.CreateCounter<long>("archix_db_ops_total");
            _failed = meter.CreateCounter<long>("archix_db_ops_failed_total");
            _durationMs = meter.CreateHistogram<double>("archix_db_op_duration_ms");
        }

        private void Begin(Guid key) => _startTicks[key] = Stopwatch.GetTimestamp();

        private void End(Guid key, bool success)
        {
            if (_startTicks.TryRemove(key, out var start))
            {
                var elapsed = Stopwatch.GetTimestamp() - start;
                var ms = elapsed * 1000.0 / Stopwatch.Frequency;
                _durationMs.Record(ms);
            }
            _total.Add(1);
            if (!success) _failed.Add(1);
        }

        // --- NonQuery ---
        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            Begin(eventData.CommandId);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Begin(eventData.CommandId);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            End(eventData.CommandId, success: true);
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            End(eventData.CommandId, success: true);
            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        // --- Reader ---
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            Begin(eventData.CommandId);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Begin(eventData.CommandId);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            End(eventData.CommandId, success: true);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            End(eventData.CommandId, success: true);
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        // --- Scalar ---
        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result)
        {
            Begin(eventData.CommandId);
            return base.ScalarExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
        {
            Begin(eventData.CommandId);
            return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override object? ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result)
        {
            End(eventData.CommandId, success: true);
            return base.ScalarExecuted(command, eventData, result);
        }

        public override ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
        {
            End(eventData.CommandId, success: true);
            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        // --- Failures ---
        public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            End(eventData.CommandId, success: false);
            base.CommandFailed(command, eventData);
        }
    }
}
