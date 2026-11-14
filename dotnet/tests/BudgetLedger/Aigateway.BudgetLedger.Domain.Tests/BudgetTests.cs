using Aigateway.BudgetLedger.Domain.Aggregates;
using Aigateway.BudgetLedger.Domain.Entities;
using Aigateway.BudgetLedger.Domain.Events;
using Aigateway.BudgetLedger.Domain.Exceptions;
using Xunit;

namespace Aigateway.BudgetLedger.Domain.Tests;

public class BudgetTests
{
    private static Budget OpenBudget(long limitAmount = 1_000) =>
        Budget.OpenMonthlyBudget(Guid.NewGuid(), year: 2026, month: 11, limitAmount);

    [Fact]
    public void OpenMonthlyBudget_WhenValidInput_OpensBudgetWithFullRemainingAmount()
    {
        var teamId = Guid.NewGuid();

        var budget = Budget.OpenMonthlyBudget(teamId, year: 2026, month: 11, limitAmount: 10_000);

        Assert.NotEqual(Guid.Empty, budget.Id);
        Assert.Equal(teamId, budget.TeamId);
        Assert.Equal(11, budget.Month);
        Assert.Equal(10_000, budget.RemainingAmount);
        Assert.Empty(budget.Reservations);
        Assert.IsType<MonthlyBudgetOpened>(Assert.Single(budget.DomainEvents));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void OpenMonthlyBudget_WhenMonthOutOfRange_ThrowsArgumentOutOfRangeException(int invalidMonth)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Budget.OpenMonthlyBudget(Guid.NewGuid(), year: 2026, invalidMonth, limitAmount: 1_000));
    }

    [Fact]
    public void ReserveFunds_WhenWithinRemainingBudget_AddsReservation()
    {
        var budget = OpenBudget(limitAmount: 10_000);
        var requestId = Guid.NewGuid();

        budget.ReserveFunds(requestId, amount: 2_500);

        var reservation = Assert.Single(budget.Reservations);
        Assert.Equal(requestId, reservation.RequestId);
        Assert.Equal(ReservationStatus.Active, reservation.Status);
        Assert.Equal(7_500, budget.RemainingAmount);
        Assert.Contains(budget.DomainEvents, e => e is FundsReserved);
    }

    [Fact]
    public void ReserveFunds_WhenAmountEqualsRemainingBudget_Succeeds()
    {
        var budget = OpenBudget(limitAmount: 1_000);

        budget.ReserveFunds(Guid.NewGuid(), amount: 1_000);

        Assert.Equal(0, budget.RemainingAmount);
    }

    [Fact]
    public void ReserveFunds_WhenAmountExceedsRemainingBudget_ThrowsBudgetExceededException()
    {
        var budget = OpenBudget(limitAmount: 1_000);
        budget.ReserveFunds(Guid.NewGuid(), amount: 800);

        var ex = Assert.Throws<BudgetExceededException>(() =>
            budget.ReserveFunds(Guid.NewGuid(), amount: 300));

        Assert.Equal("budget.exceeded", ex.Code);
        Assert.Equal(300, ex.RequestedAmount);
        Assert.Equal(200, ex.RemainingAmount);
        Assert.Single(budget.Reservations);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ReserveFunds_WhenAmountIsZeroOrNegative_ThrowsInvalidAmountException(long invalidAmount)
    {
        var budget = OpenBudget();

        var ex = Assert.Throws<InvalidAmountException>(() =>
            budget.ReserveFunds(Guid.NewGuid(), invalidAmount));

        Assert.Equal("budget.invalid_amount", ex.Code);
        Assert.Equal(invalidAmount, ex.AttemptedAmount);
        Assert.Empty(budget.Reservations);
    }

    [Fact]
    public void ReserveFunds_WhenSameRequestReservedTwice_ThrowsDuplicateReservationException()
    {
        var budget = OpenBudget();
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 100);

        var ex = Assert.Throws<DuplicateReservationException>(() =>
            budget.ReserveFunds(requestId, amount: 100));

        Assert.Equal(requestId, ex.RequestId);
        Assert.Equal(100, budget.ReservedAmount);
    }

    [Fact]
    public void ChargeActualCost_WhenWithinReservation_ChargesCostAndFreesRest()
    {
        var budget = OpenBudget(limitAmount: 10_000);
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 1_000);

        budget.ChargeActualCost(requestId, actualAmount: 750);

        Assert.Empty(budget.Reservations);
        Assert.Equal(750, budget.ChargedAmount);
        Assert.Equal(9_250, budget.RemainingAmount);
        var charged = Assert.Single(budget.DomainEvents.OfType<RequestCostCharged>());
        Assert.False(charged.ExceededReservation);
    }

    [Fact]
    public void ChargeActualCost_WhenHigherThanReserved_ChargesTrueCostAndFlagsForFinance()
    {
        var budget = OpenBudget(limitAmount: 10_000);
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 400);

        budget.ChargeActualCost(requestId, actualAmount: 500);

        Assert.Equal(500, budget.ChargedAmount);
        var charged = Assert.Single(budget.DomainEvents.OfType<RequestCostCharged>());
        Assert.True(charged.ExceededReservation);
    }

    [Fact]
    public void ChargeActualCost_WhenReservationMissing_ThrowsReservationNotFoundException()
    {
        var budget = OpenBudget();
        var missingRequestId = Guid.NewGuid();

        var ex = Assert.Throws<ReservationNotFoundException>(() =>
            budget.ChargeActualCost(missingRequestId, actualAmount: 100));

        Assert.Equal("budget.reservation_not_found", ex.Code);
        Assert.Equal(missingRequestId, ex.RequestId);
        Assert.Equal(0, budget.ChargedAmount);
    }

    [Fact]
    public void ReleaseReservedFunds_WhenReservationActive_FreesFundsWithoutCharging()
    {
        var budget = OpenBudget(limitAmount: 2_000);
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 500);

        budget.ReleaseReservedFunds(requestId);

        Assert.Empty(budget.Reservations);
        Assert.Equal(0, budget.ChargedAmount);
        Assert.Equal(2_000, budget.RemainingAmount);
    }

    [Fact]
    public void ReleaseReservedFunds_WhenReservationFrozenAsUncertain_ThrowsUncertainReservationReleaseException()
    {
        var budget = OpenBudget();
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 300);
        budget.FreezeReservation(requestId);

        var ex = Assert.Throws<UncertainReservationReleaseException>(() =>
            budget.ReleaseReservedFunds(requestId));

        Assert.Equal(requestId, ex.RequestId);
        Assert.Equal(300, ex.ReservedAmount);
        Assert.Equal(300, budget.ReservedAmount);
    }

    [Fact]
    public void ChargeFullReservation_WhenReservationUncertain_ChargesWholeReservedAmount()
    {
        var budget = OpenBudget(limitAmount: 10_000);
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 600);
        budget.FreezeReservation(requestId);

        budget.ChargeFullReservation(requestId);

        Assert.Empty(budget.Reservations);
        Assert.Equal(600, budget.ChargedAmount);
        Assert.Contains(budget.DomainEvents, e => e is FullReservationCharged);
    }

    [Fact]
    public void ChargeFullReservation_WhenReservationStillActive_ThrowsReservationNotUncertainException()
    {
        var budget = OpenBudget();
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 200);

        var ex = Assert.Throws<ReservationNotUncertainException>(() =>
            budget.ChargeFullReservation(requestId));

        Assert.Equal(requestId, ex.RequestId);
        Assert.Equal(0, budget.ChargedAmount);
    }

    [Theory]
    [InlineData(799, new int[0])]
    [InlineData(800, new[] { 80 })]
    [InlineData(900, new[] { 80, 90 })]
    [InlineData(1_000, new[] { 80, 90, 100 })]
    public void ChargeActualCost_WhenUsageCrossesThresholds_RaisesBudgetThresholdReached(long actualAmount, int[] expectedThresholds)
    {
        var budget = OpenBudget(limitAmount: 1_000);
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 1_000);

        budget.ChargeActualCost(requestId, actualAmount);

        var reached = budget.DomainEvents
            .OfType<BudgetThresholdReached>()
            .Select(e => e.ThresholdPercentage)
            .ToArray();
        Assert.Equal(expectedThresholds, reached);
    }
}
