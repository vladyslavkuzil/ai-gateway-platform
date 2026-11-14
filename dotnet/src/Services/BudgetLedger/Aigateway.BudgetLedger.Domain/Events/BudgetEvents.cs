namespace Aigateway.BudgetLedger.Domain.Events;

public sealed record MonthlyBudgetOpened(Guid BudgetId, Guid TeamId, int Year, int Month, long LimitAmount) : IDomainEvent;

public sealed record FundsReserved(Guid BudgetId, Guid RequestId, long Amount) : IDomainEvent;

public sealed record RequestCostCharged(Guid BudgetId, Guid RequestId, long ReservedAmount, long ActualAmount) : IDomainEvent
{
    public bool ExceededReservation => ActualAmount > ReservedAmount;
}

public sealed record ReservedFundsReleased(Guid BudgetId, Guid RequestId, long Amount) : IDomainEvent;

public sealed record ReservationFrozenAsUncertain(Guid BudgetId, Guid RequestId, long Amount) : IDomainEvent;

public sealed record FullReservationCharged(Guid BudgetId, Guid RequestId, long Amount) : IDomainEvent;

public sealed record BudgetThresholdReached(Guid BudgetId, Guid TeamId, int ThresholdPercentage) : IDomainEvent;
