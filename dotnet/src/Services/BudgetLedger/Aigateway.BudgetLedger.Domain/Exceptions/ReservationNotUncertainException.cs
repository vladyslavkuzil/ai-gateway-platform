namespace Aigateway.BudgetLedger.Domain.Exceptions;

public sealed class ReservationNotUncertainException : DomainException
{
    public Guid RequestId { get; }

    public ReservationNotUncertainException(Guid requestId)
        : base(
            code: "budget.reservation_not_uncertain",
            message: $"Full reservation can only be charged for a reservation frozen as uncertain (request '{requestId}').")
    {
        RequestId = requestId;
    }
}
