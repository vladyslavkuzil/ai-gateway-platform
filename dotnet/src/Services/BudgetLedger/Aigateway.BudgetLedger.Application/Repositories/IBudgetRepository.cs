using Aigateway.BudgetLedger.Domain.Aggregates;

namespace Aigateway.BudgetLedger.Application.Repositories;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid budgetId, CancellationToken cancellationToken = default);
    Task SaveAsync(Budget budget, CancellationToken cancellationToken = default);
}
