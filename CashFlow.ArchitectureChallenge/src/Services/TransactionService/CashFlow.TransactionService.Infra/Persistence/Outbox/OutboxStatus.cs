namespace CashFlow.TransactionService.Infra.Persistence.Outbox;

public enum OutboxStatus : short
{
    Pending = 1,
    Processed = 2,
    Failed = 3
}