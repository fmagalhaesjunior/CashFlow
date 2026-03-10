using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CashFlow.BalanceService.Infrastructure.Observability;

public sealed class BalanceConsumerMetrics
{
    private readonly Counter<long> _messagesReceived;
    private readonly Counter<long> _messagesProcessed;
    private readonly Counter<long> _messagesErrored;
    private readonly Counter<long> _messagesIdempotent;
    private readonly Histogram<double> _processingDurationMs;

    public BalanceConsumerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("CashFlow.BalanceService.Consumer");

        _messagesReceived = meter.CreateCounter<long>("balance_consumer_messages_received");
        _messagesProcessed = meter.CreateCounter<long>("balance_consumer_messages_processed");
        _messagesErrored = meter.CreateCounter<long>("balance_consumer_messages_errored");
        _messagesIdempotent = meter.CreateCounter<long>("balance_consumer_messages_idempotent");
        _processingDurationMs = meter.CreateHistogram<double>("balance_consumer_processing_duration_ms");
    }

    public void RecordMessageReceived()
    {
        _messagesReceived.Add(1);
    }

    public void RecordSuccess(long startedAtTimestamp)
    {
        _messagesProcessed.Add(1);
        _processingDurationMs.Record(GetElapsedMilliseconds(startedAtTimestamp));
    }

    public void RecordError(long startedAtTimestamp)
    {
        _messagesErrored.Add(1);
        _processingDurationMs.Record(GetElapsedMilliseconds(startedAtTimestamp));
    }

    public void RecordIdempotent(long startedAtTimestamp)
    {
        _messagesIdempotent.Add(1);
        _processingDurationMs.Record(GetElapsedMilliseconds(startedAtTimestamp));
    }

    private static double GetElapsedMilliseconds(long startedAtTimestamp)
    {
        return Stopwatch.GetElapsedTime(startedAtTimestamp).TotalMilliseconds;
    }
}