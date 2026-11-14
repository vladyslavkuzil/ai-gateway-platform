namespace Aigateway.BudgetLedger.Domain.Exceptions;

public sealed class ReservationNotFoundException : DomainException
{
    public Guid RequestId { get; }

    public ReservationNotFoundException(Guid requestId)
        : base(
            code: "budget.reservation_not_found",
            message: $"No reserved funds found for request '{requestId}'.")
    {
        RequestId = requestId;
    }
}
