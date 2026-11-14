using Aigateway.BudgetLedger.Application.Repositories;
using Aigateway.BudgetLedger.Application.Services;
using Aigateway.BudgetLedger.Domain.Aggregates;
using Aigateway.BudgetLedger.Domain.Exceptions;
using Moq;
using Xunit;

namespace Aigateway.BudgetLedger.Application.Tests;

public class BudgetServiceTests
{
    private readonly Mock<IBudgetRepository> _repositoryMock = new();
    private readonly BudgetService _sut;

    public BudgetServiceTests()
    {
        _sut = new BudgetService(_repositoryMock.Object);
    }

    private Budget GivenExistingBudget(long limitAmount)
    {
        var budget = Budget.OpenMonthlyBudget(Guid.NewGuid(), year: 2026, month: 11, limitAmount);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(budget.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        return budget;
    }

    [Fact]
    public async Task OpenMonthlyBudgetAsync_WhenValidInput_OpensAndSavesBudget()
    {
        var teamId = Guid.NewGuid();

        var result = await _sut.OpenMonthlyBudgetAsync(teamId, year: 2026, month: 11, limitAmount: 5_000);

        Assert.NotEqual(Guid.Empty, result.BudgetId);
        Assert.Equal(teamId, result.TeamId);
        Assert.Equal(5_000, result.RemainingAmount);
        _repositoryMock.Verify(
            r => r.SaveAsync(It.Is<Budget>(b => b.TeamId == teamId && b.LimitAmount == 5_000), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReserveFundsAsync_WhenBudgetExists_ReservesAndSavesBudget()
    {
        var budget = GivenExistingBudget(limitAmount: 10_000);

        var result = await _sut.ReserveFundsAsync(budget.Id, Guid.NewGuid(), amount: 1_200);

        Assert.Equal(1_200, result.ReservedAmount);
        Assert.Equal(8_800, result.RemainingAmount);
        _repositoryMock.Verify(r => r.SaveAsync(budget, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReserveFundsAsync_WhenBudgetDoesNotExist_ThrowsBudgetNotFoundAndNeverSaves()
    {
        var missingBudgetId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(missingBudgetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        var ex = await Assert.ThrowsAsync<BudgetNotFoundException>(
            () => _sut.ReserveFundsAsync(missingBudgetId, Guid.NewGuid(), amount: 100));

        Assert.Equal(missingBudgetId, ex.BudgetId);
        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReserveFundsAsync_WhenBudgetExceeded_PropagatesExceptionAndNeverSaves()
    {
        var budget = GivenExistingBudget(limitAmount: 500);
        budget.ReserveFunds(Guid.NewGuid(), amount: 400);

        await Assert.ThrowsAsync<BudgetExceededException>(
            () => _sut.ReserveFundsAsync(budget.Id, Guid.NewGuid(), amount: 200));

        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChargeActualCostAsync_WhenReservationExists_ChargesAndSavesBudget()
    {
        var budget = GivenExistingBudget(limitAmount: 10_000);
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 1_000);

        var result = await _sut.ChargeActualCostAsync(budget.Id, requestId, actualAmount: 750);

        Assert.Equal(750, result.ChargedAmount);
        Assert.Empty(result.Reservations);
        _repositoryMock.Verify(r => r.SaveAsync(budget, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReleaseReservedFundsAsync_WhenReservationUncertain_PropagatesExceptionAndNeverSaves()
    {
        var budget = GivenExistingBudget(limitAmount: 10_000);
        var requestId = Guid.NewGuid();
        budget.ReserveFunds(requestId, amount: 300);
        budget.FreezeReservation(requestId);

        await Assert.ThrowsAsync<UncertainReservationReleaseException>(
            () => _sut.ReleaseReservedFundsAsync(budget.Id, requestId));

        _repositoryMock.Verify(r => r.SaveAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBudgetAsync_WhenBudgetDoesNotExist_ReturnsNull()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        var result = await _sut.GetBudgetAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
