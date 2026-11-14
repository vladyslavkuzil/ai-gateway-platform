namespace Aigateway.BudgetLedger.Application.DTOs;

public sealed record ReservationDto(Guid RequestId, long Amount, string Status);

public sealed record RemainingTeamBudgetDto(
    Guid BudgetId,
    Guid TeamId,
    int Year,
    int Month,
    long LimitAmount,
    long ChargedAmount,
    long ReservedAmount,
    long RemainingAmount,
    IReadOnlyList<ReservationDto> Reservations);
