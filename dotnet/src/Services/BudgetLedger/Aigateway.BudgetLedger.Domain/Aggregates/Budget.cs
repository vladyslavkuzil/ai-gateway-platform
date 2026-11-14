using Aigateway.BudgetLedger.Domain.Entities;
using Aigateway.BudgetLedger.Domain.Events;
using Aigateway.BudgetLedger.Domain.Exceptions;

namespace Aigateway.BudgetLedger.Domain.Aggregates;

public sealed class Budget
{
    private static readonly int[] AlertThresholdPercentages = [80, 90, 100];

    private readonly List<Reservation> _reservations = new();
    private readonly List<IDomainEvent> _domainEvents = new();
    private readonly HashSet<int> _reachedThresholds = new();

    public Guid Id { get; }
    public Guid TeamId { get; }
    public int Year { get; }
    public int Month { get; }
    public long LimitAmount { get; }
    public long ChargedAmount { get; private set; }
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public long ReservedAmount => _reservations.Sum(r => r.Amount);
    public long RemainingAmount => LimitAmount - ChargedAmount - ReservedAmount;

    private Budget(Guid teamId, int year, int month, long limitAmount)
    {
        Id = Guid.NewGuid();
        TeamId = teamId;
        Year = year;
        Month = month;
        LimitAmount = limitAmount;
    }

    public static Budget OpenMonthlyBudget(Guid teamId, int year, int month, long limitAmount)
    {
        if (teamId == Guid.Empty)
            throw new ArgumentException("TeamId cannot be empty.", nameof(teamId));

        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

        if (limitAmount <= 0)
            throw new InvalidAmountException(limitAmount);

        var budget = new Budget(teamId, year, month, limitAmount);
        budget._domainEvents.Add(new MonthlyBudgetOpened(budget.Id, teamId, year, month, limitAmount));
        return budget;
    }

    public void ReserveFunds(Guid requestId, long amount)
    {
        if (amount <= 0)
            throw new InvalidAmountException(amount);

        if (_reservations.Any(r => r.RequestId == requestId))
            throw new DuplicateReservationException(requestId);

        if (amount > RemainingAmount)
            throw new BudgetExceededException(Id, LimitAmount, ChargedAmount, ReservedAmount, amount);

        _reservations.Add(new Reservation(requestId, amount));
        _domainEvents.Add(new FundsReserved(Id, requestId, amount));
    }

    public void ChargeActualCost(Guid requestId, long actualAmount)
    {
        if (actualAmount < 0)
            throw new InvalidAmountException(actualAmount);

        var reservation = FindReservationOrThrow(requestId);

        _reservations.Remove(reservation);
        ChargedAmount += actualAmount;
        _domainEvents.Add(new RequestCostCharged(Id, requestId, reservation.Amount, actualAmount));

        CheckBudgetUsage();
    }

    public void ReleaseReservedFunds(Guid requestId)
    {
        var reservation = FindReservationOrThrow(requestId);

        if (reservation.Status == ReservationStatus.Uncertain)
            throw new UncertainReservationReleaseException(requestId, reservation.Amount);

        _reservations.Remove(reservation);
        _domainEvents.Add(new ReservedFundsReleased(Id, requestId, reservation.Amount));
    }

    public void FreezeReservation(Guid requestId)
    {
        var reservation = FindReservationOrThrow(requestId);

        if (reservation.Status == ReservationStatus.Uncertain)
            return;

        reservation.Freeze();
        _domainEvents.Add(new ReservationFrozenAsUncertain(Id, requestId, reservation.Amount));
    }

    public void ChargeFullReservation(Guid requestId)
    {
        var reservation = FindReservationOrThrow(requestId);

        if (reservation.Status != ReservationStatus.Uncertain)
            throw new ReservationNotUncertainException(requestId);

        _reservations.Remove(reservation);
        ChargedAmount += reservation.Amount;
        _domainEvents.Add(new FullReservationCharged(Id, requestId, reservation.Amount));

        CheckBudgetUsage();
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void CheckBudgetUsage()
    {
        var usedPercentage = ChargedAmount * 100 / LimitAmount;

        foreach (var threshold in AlertThresholdPercentages)
        {
            if (usedPercentage >= threshold && _reachedThresholds.Add(threshold))
                _domainEvents.Add(new BudgetThresholdReached(Id, TeamId, threshold));
        }
    }

    private Reservation FindReservationOrThrow(Guid requestId) =>
        _reservations.FirstOrDefault(r => r.RequestId == requestId)
        ?? throw new ReservationNotFoundException(requestId);
}
