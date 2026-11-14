namespace Aigateway.BudgetLedger.Presentation.Contracts;

public sealed record OpenMonthlyBudgetRequest(Guid TeamId, int Year, int Month, long LimitAmount);

public sealed record ReserveFundsRequest(Guid RequestId, long Amount);

public sealed record ChargeActualCostRequest(Guid RequestId, long ActualAmount);
