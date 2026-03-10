using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace CashFlow.BalanceService.Infrastructure.Messaging.RabbitMq;

public sealed class ConsumerFailurePublisher : IConsumerFailurePublisher
{
    private readonly RabbitMqConsumerOptions _options;
    private readonly ILogger<ConsumerFailurePublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public ConsumerFailurePublisher(
        IOptions<RabbitMqConsumerOptions> options,
        ILogger<ConsumerFailurePublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
    }

    public async Task PublishToRetryAsync(
        string payload,
        string eventType,
        string? eventId,
        string? correlationId,
        int retryCount,
        CancellationToken cancellationToken)
    {
        var body = Encoding.UTF8.GetBytes(payload);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = eventType,
            MessageId = eventId,
            CorrelationId = correlationId,
            Expiration = _options.RetryDelayMilliseconds.ToString(),
            Headers = new Dictionary<string, object?>
            {
                ["event_id"] = eventId ?? string.Empty,
                ["event_type"] = eventType,
                ["correlation_id"] = correlationId ?? string.Empty,
                ["consumer_retry_count"] = retryCount
            }
        };

        await _channel.BasicPublishAsync(
            exchange: _options.RetryExchangeName,
            routingKey: _options.RetryRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogWarning(
            "Message published to retry queue. EventId: {EventId}, RetryCount: {RetryCount}, CorrelationId: {CorrelationId}",
            eventId,
            retryCount,
            correlationId);
    }

    public async Task PublishToDeadLetterAsync(
        string payload,
        string eventType,
        string? eventId,
        string? correlationId,
        int retryCount,
        string reason,
        CancellationToken cancellationToken)
    {
        var body = Encoding.UTF8.GetBytes(payload);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = eventType,
            MessageId = eventId,
            CorrelationId = correlationId,
            Headers = new Dictionary<string, object?>
            {
                ["event_id"] = eventId ?? string.Empty,
                ["event_type"] = eventType,
                ["correlation_id"] = correlationId ?? string.Empty,
                ["consumer_retry_count"] = retryCount,
                ["dead_letter_reason"] = reason
            }
        };

        await _channel.BasicPublishAsync(
            exchange: _options.DeadLetterExchangeName,
            routingKey: _options.DeadLetterRoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogError(
            "Message published to DLQ. EventId: {EventId}, RetryCount: {RetryCount}, CorrelationId: {CorrelationId}, Reason: {Reason}",
            eventId,
            retryCount,
            correlationId,
            reason);
    }
}