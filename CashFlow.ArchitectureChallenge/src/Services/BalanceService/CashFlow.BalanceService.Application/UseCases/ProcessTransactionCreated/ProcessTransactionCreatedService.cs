using CashFlow.BalanceService.Application.Abstractions.Persistence;
using CashFlow.BalanceService.Application.Extensions;
using CashFlow.BalanceService.Application.Models;
using CashFlow.BalanceService.Application.Parsers;
using CashFlow.BalanceService.Domain.Entities;
using CashFlow.BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace CashFlow.BalanceService.Application.UseCases.ProcessTransactionCreated;

public sealed class ProcessTransactionCreatedService
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;
    private readonly IProcessedEventRepository _processedEventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessTransactionCreatedService> _logger;

    public ProcessTransactionCreatedService(
        IDailyBalanceRepository dailyBalanceRepository,
        IProcessedEventRepository processedEventRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessTransactionCreatedService> logger)
    {
        _dailyBalanceRepository = dailyBalanceRepository;
        _processedEventRepository = processedEventRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ProcessAsync(
        TransactionCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing transaction created event. EventId: {EventId}, TransactionId: {TransactionId}, Type: {Type}",
            integrationEvent.EventId,
            integrationEvent.TransactionId,
            integrationEvent.Type);

        var alreadyProcessed = await _processedEventRepository.ExistsAsync(
            integrationEvent.EventId,
            cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "Event already processed. EventId: {EventId}",
                integrationEvent.EventId);

            return;
        }

        var balanceDate = DateOnly.FromDateTime(integrationEvent.Timestamp.UtcDateTimeSafe());
        var transactionType = TransactionTypeParser.Parse(integrationEvent.Type);

        var dailyBalance = await _dailyBalanceRepository.GetByDateAsync(
            balanceDate,
            cancellationToken);

        if (dailyBalance is null)
        {
            dailyBalance = DailyBalance.Create(balanceDate);
            await _dailyBalanceRepository.AddAsync(dailyBalance, cancellationToken);

            _logger.LogInformation(
                "Daily balance created. Date: {Date}",
                balanceDate);
        }

        switch (transactionType)
        {
            case BalanceTransactionType.Credit:
                dailyBalance.ApplyCredit(integrationEvent.Amount);
                break;

            case BalanceTransactionType.Debit:
                dailyBalance.ApplyDebit(integrationEvent.Amount);
                break;

            default:
                throw new InvalidOperationException($"Unsupported transaction type: {integrationEvent.Type}");
        }

        await _processedEventRepository.AddAsync(
            new ProcessedEvent(integrationEvent.EventId, DateTime.UtcNow),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Daily balance updated successfully. EventId: {EventId}, Date: {Date}, TotalCredit: {TotalCredit}, TotalDebit: {TotalDebit}, Balance: {Balance}",
            integrationEvent.EventId,
            dailyBalance.Date,
            dailyBalance.TotalCredit,
            dailyBalance.TotalDebit,
            dailyBalance.Balance);
    }
}