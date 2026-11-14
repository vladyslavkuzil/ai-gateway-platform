namespace Aigateway.BudgetLedger.Domain.Exceptions;

public sealed class InvalidAmountException : DomainException
{
    public long AttemptedAmount { get; }

    public InvalidAmountException(long attemptedAmount)
        : base(
            code: "budget.invalid_amount",
            message: $"Amount {attemptedAmount} is not allowed.")
    {
        AttemptedAmount = attemptedAmount;
    }
}
