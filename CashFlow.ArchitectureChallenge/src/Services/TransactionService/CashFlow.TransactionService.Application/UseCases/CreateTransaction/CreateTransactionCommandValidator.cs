using FluentValidation;

namespace CashFlow.TransactionService.Application.UseCases.CreateTransaction;

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Type)
            .Must(x => x is 1 or 2)
            .WithMessage("Transaction type must be 1 (Credit) or 2 (Debit).");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(200);
    }
}