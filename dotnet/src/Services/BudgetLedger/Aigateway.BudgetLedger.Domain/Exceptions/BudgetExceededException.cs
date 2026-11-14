namespace Aigateway.BudgetLedger.Domain.Exceptions;

public sealed class BudgetExceededException : DomainException
{
    public Guid BudgetId { get; }
    public long LimitAmount { get; }
    public long ChargedAmount { get; }
    public long ReservedAmount { get; }
    public long RequestedAmount { get; }
    public long RemainingAmount { get; }

    public BudgetExceededException(
        Guid budgetId,
        long limitAmount,
        long chargedAmount,
        long reservedAmount,
        long requestedAmount)
        : base(
            code: "budget.exceeded",
            message: $"Cannot reserve {requestedAmount}. Remaining team budget is {limitAmount - chargedAmount - reservedAmount} (limit={limitAmount}, charged={chargedAmount}, reserved={reservedAmount}).")
    {
        BudgetId = budgetId;
        LimitAmount = limitAmount;
        ChargedAmount = chargedAmount;
        ReservedAmount = reservedAmount;
        RequestedAmount = requestedAmount;
        RemainingAmount = limitAmount - chargedAmount - reservedAmount;
    }
}
