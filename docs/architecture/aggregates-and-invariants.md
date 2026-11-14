# Aggregates and Value Objects – AI Gateway Platform

This document is based on our Event Storming board from Lab 1. Names of aggregates, commands and events below are the same as on the sticky notes (Ubiquitous Language).

## Aggregates from the board

| Bounded Context | Aggregate (root) | Commands from the board | Events from the board |
| --- | --- | --- | --- |
| Access | `Organization` | Register organization, Create team | Organization registered, Team created |
| Access | `AccessKey` | Issue access key, Revoke access key | Access key issued, Access key revoked |
| Governance | `UsagePolicy` | Define team usage policy, Check request against policy | Team usage policy defined, Request rejected by policy |
| Governance | `ModelCatalog` | Publish model price list | Model price list published |
| Governance | `RoutePlan` | Choose provider route | Provider route chosen |
| Budget | `Budget` | Open monthly budget, Reserve funds, Charge actual cost, Release reserved funds, Charge full reservation, Record cost correction | Monthly budget opened, Funds reserved, Budget exceeded, Request cost charged, Reserved funds released, Reservation frozen as uncertain, Full reservation charged, Cost correction recorded, Budget threshold reached |
| Requests | `CompletionRequest` | Submit completion request, Deliver answer | Completion request submitted, Duplicate request detected, Answer delivered to client |
| Providers | `ProviderAttempt` | Send prompt to provider, Record provider answer, Fail over to backup provider | Prompt sent to provider, Answer received from provider, Request moved to backup provider, Provider outcome became unknown |
| Providers | `ProviderHealth` | Mark provider unavailable | Provider marked unavailable |
| Notifications | `Alert` | Send budget alert | Budget alert sent |

Aggregates reference each other only by strongly typed identifiers (for example `Budget` stores a `RequestId`, never a `CompletionRequest` object).

The main aggregate (`Budget`) and its reservation lifecycle are described in [stateDiagram-budget-reservation.md](stateDiagram-budget-reservation.md).

## Value Objects

All Value Objects are immutable: fields are final, there are no setters, and every operation returns a new instance. Invalid input is rejected in the factory method, so an invalid object can never exist.

| Value Object | Replaces | Attributes and types | Self-validation (rejected at creation) | Example operation (returns a new instance) |
| --- | --- | --- | --- | --- |
| `Money` | `long` / `decimal` amount + `String` currency | `long amountMicros` (1 unit = 1 000 000 micros), `String currency` (ISO 4217 code) | negative `amountMicros`; currency that is `null`, blank or not exactly 3 letters | `Money add(Money other)` – throws `CurrencyMismatchException` if currencies differ; `Money subtract(Money other)` – throws `NegativeMoneyException` if the result would be below zero |
| `TokenCount` | `int` for prompt / completion tokens | `int value` | negative value; value above the model's context window limit (1 000 000) | `TokenCount plus(TokenCount other)` – used to add input and output tokens into total usage |
| `TokenPrice` | two `decimal` prices copied from the price list | `Money inputPricePerMillion`, `Money outputPricePerMillion` | price equal to or below zero; input and output prices in different currencies | `Money costOf(TokenCount input, TokenCount output)` – returns a new `Money`, always rounded **up** to the next micro, so a reservation is never too small |
| `CostEstimate` | raw `long estimate` sent to the budget | `TokenCount maxInputTokens`, `TokenCount maxOutputTokens`, `TokenPrice priceSnapshot`, `Money total` | `maxOutputTokens` equal to 0; `total` equal to 0; `total` that does not match `priceSnapshot.costOf(...)` | `CostEstimate repricedFor(TokenPrice backupProviderPrice)` – new estimate when the request is moved to a backup provider (board: *Request moved to backup provider*) |
| `BudgetPeriod` | pair of `Instant periodStart`, `Instant periodEnd` | `Instant start` (inclusive), `Instant end` (exclusive), always UTC | `end` not after `start`; `start` not on the first day of a month at 00:00 UTC; period longer than one calendar month | `BudgetPeriod next()` – returns the following calendar month, used by *Open monthly budget*; `boolean contains(Instant t)` |
| `IdempotencyKey` | `String` from the `Idempotency-Key` header | `String value` | `null`, blank, longer than 255 characters, contains control characters | `IdempotencyKey scopedTo(TeamId team)` – returns a key unique per team, because the same key from two different teams means two different requests |

### Why these are Value Objects and not Entities

- Two `Money` values of `4_500_000` micros USD are the same – it does not matter "which" amount it is.
- A `TokenPrice` is copied into the reservation as a snapshot. When the platform operator publishes a new price list (*Model price list published*), old reservations keep their old `TokenPrice`, so nothing changes for requests that are already running.
- `BudgetPeriod` is only a description of time; the `Budget` that uses it has its own identity (`BudgetId`).

### Bugs these Value Objects prevent in our domain

| Without Value Object | Bug | Prevented by |
| --- | --- | --- |
| `long estimate`, `long actual` | Adding a USD price to a budget in EUR | `Money.add` throws `CurrencyMismatchException` |
| `int maxTokens` | `max_tokens = 0` or `-5` creates a reservation of 0, so the provider call is never covered by the budget | `CostEstimate` rejects 0 output tokens and 0 total |
| price read live from the catalog | Price change during a running request changes its cost | `TokenPrice` snapshot inside `CostEstimate` |
| two `Instant` fields | `periodEnd` before `periodStart`, request reserved against a wrong month | `BudgetPeriod` validation and `contains` |

## Strongly typed identifiers

Each identifier wraps a `UUID` (Java `record`). A method like `reserve(RequestId, TeamId, BudgetId, CostEstimate)` cannot be called with the arguments in the wrong order, because the compiler sees three different types instead of three `UUID`s.

| Identifier | Entity / aggregate | Bounded Context |
| --- | --- | --- |
| `OrganizationId` | `Organization` | Access |
| `TeamId` | team inside `Organization` | Access |
| `AccessKeyId` | `AccessKey` | Access |
| `UsagePolicyId` | `UsagePolicy` | Governance |
| `RoutePlanId` | `RoutePlan` | Governance |
| `ModelId` | model entry in `ModelCatalog` | Governance |
| `RequestId` | `CompletionRequest` (also used as correlation id in all contexts) | Requests |
| `BudgetId` | `Budget` | Budget |
| `ReservationId` | `Reservation` (internal entity of `Budget`) | Budget |
| `ProviderAttemptId` | `ProviderAttempt` | Providers |
| `ProviderId` | `ProviderHealth` | Providers |
| `AlertId` | `Alert` | Notifications |

Example contract:

```java
public record RequestId(UUID value) {
    public RequestId {
        Objects.requireNonNull(value);
    }

    public static RequestId newId() {
        return new RequestId(UUID.randomUUID());
    }
}
```
