namespace Aigateway.BudgetLedger.Domain.Exceptions;

public sealed class DuplicateReservationException : DomainException
{
    public Guid RequestId { get; }

    public DuplicateReservationException(Guid requestId)
        : base(
            code: "budget.duplicate_reservation",
            message: $"Funds are already reserved for request '{requestId}'.")
    {
        RequestId = requestId;
    }
}
