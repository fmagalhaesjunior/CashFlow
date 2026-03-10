using CashFlow.BuildingBlocks.Application.Messaging;
using CashFlow.BuildingBlocks.Contracts.Events;
using CashFlow.BuildingBlocks.Contracts.Messaging;
using CashFlow.TransactionService.Application.Abstractions;
using CashFlow.TransactionService.Application.Abstractions.Messaging;
using CashFlow.TransactionService.Application.Abstractions.Persistence;
using CashFlow.TransactionService.Domain.Entities;
using CashFlow.TransactionService.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CashFlow.TransactionService.Application.UseCases.CreateTransaction;

public sealed class CreateTransactionCommandHandler
    : ICommandHandler<CreateTransactionCommand, CreateTransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateTransactionCommandHandler> _logger;

    public CreateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<CreateTransactionCommandHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<CreateTransactionResponse> Handle(
        CreateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var correlationId = command.CorrelationId;

        _logger.LogInformation(
            "Creating transaction. CorrelationId: {CorrelationId}, Amount: {Amount}, Type: {Type}",
            correlationId,
            command.Amount,
            command.Type);

        var transactionType = (TransactionType)command.Type;

        var transaction = Transaction.Create(
            command.Amount,
            transactionType,
            command.Description,
            _dateTimeProvider.UtcNow);

        await _transactionRepository.AddAsync(transaction, cancellationToken);

        var integrationEvent = new TransactionCreatedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            TransactionId = transaction.Id,
            Amount = transaction.Amount,
            Type = transaction.Type.ToString().ToUpperInvariant(),
            Timestamp = transaction.Timestamp,
            OccurredOnUtc = _dateTimeProvider.UtcNow
        };

        var outboxMessage = new OutboxMessage
        {
            Id = integrationEvent.EventId,
            Type = nameof(TransactionCreatedIntegrationEvent),
            Payload = JsonSerializer.Serialize(integrationEvent),
            OccurredOnUtc = integrationEvent.OccurredOnUtc,
            CorrelationId = correlationId
        };

        await _outboxWriter.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Transaction created successfully. TransactionId: {TransactionId}, EventId: {EventId}, CorrelationId: {CorrelationId}",
            transaction.Id,
            outboxMessage.Id,
            outboxMessage.CorrelationId);

        return new CreateTransactionResponse
        {
            TransactionId = transaction.Id,
            Message = "Transaction created successfully.",
            Timestamp = transaction.Timestamp
        };
    }
}