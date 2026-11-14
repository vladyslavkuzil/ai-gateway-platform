namespace Aigateway.BudgetLedger.Domain.Exceptions;

public sealed class UncertainReservationReleaseException : DomainException
{
    public Guid RequestId { get; }
    public long ReservedAmount { get; }

    public UncertainReservationReleaseException(Guid requestId, long reservedAmount)
        : base(
            code: "budget.uncertain_reservation_release",
            message: $"Reservation for request '{requestId}' is frozen as uncertain; its {reservedAmount} reserved funds must stay reserved.")
    {
        RequestId = requestId;
        ReservedAmount = reservedAmount;
    }
}
