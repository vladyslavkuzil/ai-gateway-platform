using Aigateway.BudgetLedger.Domain.Exceptions;

namespace Aigateway.BudgetLedger.Domain.Entities;

public enum ReservationStatus
{
    Active,
    Uncertain
}

public sealed class Reservation
{
    public Guid RequestId { get; }
    public long Amount { get; }
    public ReservationStatus Status { get; private set; }

    internal Reservation(Guid requestId, long amount)
    {
        if (requestId == Guid.Empty)
            throw new ArgumentException("RequestId cannot be empty.", nameof(requestId));

        if (amount <= 0)
            throw new InvalidAmountException(amount);

        RequestId = requestId;
        Amount = amount;
        Status = ReservationStatus.Active;
    }

    internal void Freeze() => Status = ReservationStatus.Uncertain;
}
