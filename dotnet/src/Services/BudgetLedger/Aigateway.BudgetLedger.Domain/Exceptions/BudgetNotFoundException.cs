namespace Aigateway.BudgetLedger.Domain.Exceptions;

public sealed class BudgetNotFoundException : DomainException
{
    public Guid BudgetId { get; }

    public BudgetNotFoundException(Guid budgetId)
        : base(
            code: "budget.not_found",
            message: $"Budget with ID '{budgetId}' was not found.")
    {
        BudgetId = budgetId;
    }
}
