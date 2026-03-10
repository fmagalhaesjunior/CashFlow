using CashFlow.TransactionService.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace CashFlow.TransactionService.Infra.Messaging.RabbitMq;

public sealed class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly IEventRoutingKeyResolver _routingKeyResolver;
    private readonly ILogger<RabbitMqIntegrationEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqIntegrationEventPublisher(
        IOptions<RabbitMqOptions> options,
        IEventRoutingKeyResolver routingKeyResolver,
        ILogger<RabbitMqIntegrationEventPublisher> logger)
    {
        _options = options.Value;
        _routingKeyResolver = routingKeyResolver;
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

        _channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false).GetAwaiter().GetResult();
    }

    public async Task PublishAsync(PublishEnvelope envelope, CancellationToken cancellationToken)
    {
        var routingKey = _routingKeyResolver.Resolve(envelope.EventType);
        var body = Encoding.UTF8.GetBytes(envelope.Payload);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = envelope.EventType,
            MessageId = envelope.EventId.ToString(),
            CorrelationId = envelope.CorrelationId,
            Headers = new Dictionary<string, object?>
            {
                ["event_id"] = envelope.EventId.ToString(),
                ["event_type"] = envelope.EventType,
                ["correlation_id"] = envelope.CorrelationId ?? string.Empty
            }
        };

        await _channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Event published to RabbitMQ. EventId: {EventId}, EventType: {EventType}, RoutingKey: {RoutingKey}, CorrelationId: {CorrelationId}",
            envelope.EventId,
            envelope.EventType,
            routingKey,
            envelope.CorrelationId);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}