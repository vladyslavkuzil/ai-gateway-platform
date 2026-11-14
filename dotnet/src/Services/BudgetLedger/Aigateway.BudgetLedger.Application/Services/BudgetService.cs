using Aigateway.BudgetLedger.Application.DTOs;
using Aigateway.BudgetLedger.Application.Repositories;
using Aigateway.BudgetLedger.Domain.Aggregates;
using Aigateway.BudgetLedger.Domain.Exceptions;

namespace Aigateway.BudgetLedger.Application.Services;

public sealed class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;

    public BudgetService(IBudgetRepository budgetRepository)
    {
        _budgetRepository = budgetRepository;
    }

    public async Task<RemainingTeamBudgetDto> OpenMonthlyBudgetAsync(
        Guid teamId,
        int year,
        int month,
        long limitAmount,
        CancellationToken cancellationToken = default)
    {
        var budget = Budget.OpenMonthlyBudget(teamId, year, month, limitAmount);
        await _budgetRepository.SaveAsync(budget, cancellationToken);
        return MapToDto(budget);
    }

    public async Task<RemainingTeamBudgetDto?> GetBudgetAsync(Guid budgetId, CancellationToken cancellationToken = default)
    {
        var budget = await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
        return budget is null ? null : MapToDto(budget);
    }

    public async Task<RemainingTeamBudgetDto> ReserveFundsAsync(
        Guid budgetId,
        Guid requestId,
        long amount,
        CancellationToken cancellationToken = default)
    {
        var budget = await GetExistingBudgetOrThrowAsync(budgetId, cancellationToken);

        budget.ReserveFunds(requestId, amount);

        await _budgetRepository.SaveAsync(budget, cancellationToken);
        return MapToDto(budget);
    }

    public async Task<RemainingTeamBudgetDto> ChargeActualCostAsync(
        Guid budgetId,
        Guid requestId,
        long actualAmount,
        CancellationToken cancellationToken = default)
    {
        var budget = await GetExistingBudgetOrThrowAsync(budgetId, cancellationToken);

        budget.ChargeActualCost(requestId, actualAmount);

        await _budgetRepository.SaveAsync(budget, cancellationToken);
        return MapToDto(budget);
    }

    public async Task<RemainingTeamBudgetDto> ReleaseReservedFundsAsync(
        Guid budgetId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var budget = await GetExistingBudgetOrThrowAsync(budgetId, cancellationToken);

        budget.ReleaseReservedFunds(requestId);

        await _budgetRepository.SaveAsync(budget, cancellationToken);
        return MapToDto(budget);
    }

    private async Task<Budget> GetExistingBudgetOrThrowAsync(Guid budgetId, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
        return budget ?? throw new BudgetNotFoundException(budgetId);
    }

    private static RemainingTeamBudgetDto MapToDto(Budget budget) =>
        new(
            budget.Id,
            budget.TeamId,
            budget.Year,
            budget.Month,
            budget.LimitAmount,
            budget.ChargedAmount,
            budget.ReservedAmount,
            budget.RemainingAmount,
            budget.Reservations
                .Select(r => new ReservationDto(r.RequestId, r.Amount, r.Status.ToString()))
                .ToList());
}
