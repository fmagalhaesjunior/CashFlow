using CashFlow.BuildingBlocks.Application.Messaging;
using CashFlow.BuildingBlocks.Contracts.Events;
using CashFlow.TransactionService.Application.Abstractions;
using CashFlow.TransactionService.Application.Abstractions.Messaging;
using CashFlow.TransactionService.Application.Abstractions.Persistence;
using CashFlow.TransactionService.Domain.Entities;
using CashFlow.TransactionService.Domain.Enums;

namespace CashFlow.TransactionService.Application.UseCases.CreateTransaction;

public sealed class CreateTransactionCommandHandler
    : ICommandHandler<CreateTransactionCommand, CreateTransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _transactionRepository = transactionRepository;
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<CreateTransactionResponse> Handle(
        CreateTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var transactionType = (TransactionType)command.Type;

        var transaction = Transaction.Create(
            command.Amount,
            transactionType,
            command.Description,
            _dateTimeProvider.UtcNow);

        await _transactionRepository.AddAsync(transaction, cancellationToken);

        var integrationEvent = new TransactionCreatedIntegrationEvent
        {
            TransactionId = transaction.Id,
            Amount = transaction.Amount,
            Type = transaction.Type.ToString().ToUpperInvariant(),
            Timestamp = transaction.Timestamp,
            OccurredOnUtc = _dateTimeProvider.UtcNow
        };

        await _outboxWriter.AddAsync(
            nameof(TransactionCreatedIntegrationEvent),
            integrationEvent,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTransactionResponse
        {
            TransactionId = transaction.Id,
            Message = "Transaction created successfully.",
            Timestamp = transaction.Timestamp
        };
    }
}