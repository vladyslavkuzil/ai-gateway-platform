using Aigateway.BudgetLedger.Application.DTOs;
using Aigateway.BudgetLedger.Application.Services;
using Aigateway.BudgetLedger.Domain.Exceptions;
using Aigateway.BudgetLedger.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Aigateway.BudgetLedger.Presentation.Controllers;

[ApiController]
[Route("api/budgets")]
public sealed class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpPost]
    public async Task<ActionResult<RemainingTeamBudgetDto>> OpenMonthlyBudget(
        [FromBody] OpenMonthlyBudgetRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var budget = await _budgetService.OpenMonthlyBudgetAsync(
                request.TeamId,
                request.Year,
                request.Month,
                request.LimitAmount,
                cancellationToken);

            return CreatedAtAction(nameof(GetBudget), new { budgetId = budget.BudgetId }, budget);
        }
        catch (DomainException ex)
        {
            return MapDomainException(ex);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { code = "validation.invalid_argument", message = ex.Message });
        }
    }

    [HttpGet("{budgetId:guid}")]
    public async Task<ActionResult<RemainingTeamBudgetDto>> GetBudget(Guid budgetId, CancellationToken cancellationToken)
    {
        var budget = await _budgetService.GetBudgetAsync(budgetId, cancellationToken);
        if (budget is null)
        {
            return NotFound(new { code = "budget.not_found", message = $"Budget '{budgetId}' was not found." });
        }

        return Ok(budget);
    }

    [HttpPost("{budgetId:guid}/reservations")]
    public async Task<ActionResult<RemainingTeamBudgetDto>> ReserveFunds(
        Guid budgetId,
        [FromBody] ReserveFundsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _budgetService.ReserveFundsAsync(budgetId, request.RequestId, request.Amount, cancellationToken));
        }
        catch (DomainException ex)
        {
            return MapDomainException(ex);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { code = "validation.invalid_argument", message = ex.Message });
        }
    }

    [HttpPost("{budgetId:guid}/charges")]
    public async Task<ActionResult<RemainingTeamBudgetDto>> ChargeActualCost(
        Guid budgetId,
        [FromBody] ChargeActualCostRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _budgetService.ChargeActualCostAsync(budgetId, request.RequestId, request.ActualAmount, cancellationToken));
        }
        catch (DomainException ex)
        {
            return MapDomainException(ex);
        }
    }

    [HttpPost("{budgetId:guid}/reservations/{requestId:guid}/release")]
    public async Task<ActionResult<RemainingTeamBudgetDto>> ReleaseReservedFunds(
        Guid budgetId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _budgetService.ReleaseReservedFundsAsync(budgetId, requestId, cancellationToken));
        }
        catch (DomainException ex)
        {
            return MapDomainException(ex);
        }
    }

    private ActionResult MapDomainException(DomainException ex) => ex switch
    {
        BudgetNotFoundException or ReservationNotFoundException =>
            NotFound(new { code = ex.Code, message = ex.Message }),
        InvalidAmountException =>
            BadRequest(new { code = ex.Code, message = ex.Message }),
        DuplicateReservationException =>
            Conflict(new { code = ex.Code, message = ex.Message }),
        BudgetExceededException exceeded => UnprocessableEntity(new
        {
            code = exceeded.Code,
            message = exceeded.Message,
            limitAmount = exceeded.LimitAmount,
            chargedAmount = exceeded.ChargedAmount,
            reservedAmount = exceeded.ReservedAmount,
            requestedAmount = exceeded.RequestedAmount,
            remainingAmount = exceeded.RemainingAmount
        }),
        _ => UnprocessableEntity(new { code = ex.Code, message = ex.Message })
    };
}
