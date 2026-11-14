using Aigateway.BudgetLedger.Application.DTOs;

namespace Aigateway.BudgetLedger.Application.Services;

public interface IBudgetService
{
    Task<RemainingTeamBudgetDto> OpenMonthlyBudgetAsync(Guid teamId, int year, int month, long limitAmount, CancellationToken cancellationToken = default);
    Task<RemainingTeamBudgetDto?> GetBudgetAsync(Guid budgetId, CancellationToken cancellationToken = default);
    Task<RemainingTeamBudgetDto> ReserveFundsAsync(Guid budgetId, Guid requestId, long amount, CancellationToken cancellationToken = default);
    Task<RemainingTeamBudgetDto> ChargeActualCostAsync(Guid budgetId, Guid requestId, long actualAmount, CancellationToken cancellationToken = default);
    Task<RemainingTeamBudgetDto> ReleaseReservedFundsAsync(Guid budgetId, Guid requestId, CancellationToken cancellationToken = default);
}
