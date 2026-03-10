using CashFlow.BalanceService.Application.UseCases.ProcessTransactionCreated;
using CashFlow.BalanceService.Infrastructure.Messaging.RabbitMq;
using CashFlow.BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CashFlow.BalanceService.Infrastructure.BackgroundJobs;

public sealed class TransactionCreatedConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqConsumerOptions _options;
    private readonly ILogger<TransactionCreatedConsumerWorker> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public TransactionCreatedConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqConsumerOptions> options,
        ILogger<TransactionCreatedConsumerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey,
            cancellationToken: cancellationToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _options.PrefetchCount,
            global: false,
            cancellationToken: cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            throw new InvalidOperationException("RabbitMQ channel was not initialized.");
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var eventId = ea.BasicProperties?.MessageId;
            var correlationId = ea.BasicProperties?.CorrelationId;

            try
            {
                _logger.LogInformation(
                    "Message received from RabbitMQ. DeliveryTag: {DeliveryTag}, EventId: {EventId}, CorrelationId: {CorrelationId}",
                    ea.DeliveryTag,
                    eventId,
                    correlationId);

                var integrationEvent = JsonSerializer.Deserialize<TransactionCreatedIntegrationEvent>(json)
                    ?? throw new InvalidOperationException("Failed to deserialize TransactionCreatedIntegrationEvent.");

                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ProcessTransactionCreatedService>();

                await processor.ProcessAsync(integrationEvent, stoppingToken);

                await _channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Message acknowledged successfully. DeliveryTag: {DeliveryTag}, EventId: {EventId}",
                    ea.DeliveryTag,
                    integrationEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process RabbitMQ message. DeliveryTag: {DeliveryTag}, EventId: {EventId}, CorrelationId: {CorrelationId}",
                    ea.DeliveryTag,
                    eventId,
                    correlationId);

                await _channel.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Balance consumer started. Queue: {QueueName}, RoutingKey: {RoutingKey}, PrefetchCount: {PrefetchCount}",
            _options.QueueName,
            _options.RoutingKey,
            _options.PrefetchCount);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}