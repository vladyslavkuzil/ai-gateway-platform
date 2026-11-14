using System.Collections.Concurrent;
using Aigateway.BudgetLedger.Application.Repositories;
using Aigateway.BudgetLedger.Domain.Aggregates;

namespace Aigateway.BudgetLedger.Presentation.Infrastructure;

public sealed class InMemoryBudgetRepository : IBudgetRepository
{
    private readonly ConcurrentDictionary<Guid, Budget> _storage = new();

    public Task<Budget?> GetByIdAsync(Guid budgetId, CancellationToken cancellationToken = default)
    {
        _storage.TryGetValue(budgetId, out var budget);
        return Task.FromResult(budget);
    }

    public Task SaveAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _storage[budget.Id] = budget;
        return Task.CompletedTask;
    }
}
