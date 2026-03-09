namespace CashFlow.BuildingBlocks.Domain.Abstractions;

public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}