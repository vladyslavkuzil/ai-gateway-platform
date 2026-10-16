# Development conventions

Practical rules for day-to-day work in this monorepo. Architecture and service boundaries stay in the root [README](../README.md).

## Branch naming

Pattern:

```text
<type>/<service-or-area>[-short-detail]
```

### Types

| Prefix | Use |
| --- | --- |
| `feat/` | New service capability or feature |
| `fix/` | Bug fix |
| `chore/` | Tooling, Compose, CI, scaffolding |
| `docs/` | Documentation only |
| `refactor/` | Internal cleanup, no behavior change |

### Service / area names

Use the deployable name when possible:

`mock-provider`, `provider-service`, `inference-service`, `api-gateway`,  
`identity-service`, `tenant-service`, `policy-service`, `model-catalog-service`,  
`budget-ledger-service`, `notification-service`, `analytics-service`, `audit-service`,  
`compose`, `infra`, `contracts`, `aks`

### Examples

```text
feat/mock-provider
feat/provider-service
feat/api-gateway
fix/budget-ledger-service-reservation-race
chore/compose-healthchecks
docs/readme-aks
```

### Rules

- One primary service (or clear area) per branch.
- Prefer merging a runnable service before starting the next dependent service.
- Keep branch names lowercase, hyphen-separated, no spaces.
- Do not put ticket IDs alone as the branch name; put the service first.

### Suggested build order (first vertical slice)

1. `feat/mock-provider`
2. `feat/provider-service`
3. `feat/inference-service`
4. `feat/api-gateway`

## Commit naming

Use [Conventional Commits](https://www.conventionalcommits.org/):

```text
<type>(<scope>): <imperative summary>
```

Optional body for non-obvious *why*.

### Types

| Type | Use |
| --- | --- |
| `feat` | New user-visible or service-visible behavior |
| `fix` | Bug fix |
| `chore` | Scaffolding, deps, Compose, ignore files |
| `test` | Tests only |
| `docs` | Docs only |
| `refactor` | Restructure without behavior change |
| `build` | Maven/Docker/build pipeline |

### Scopes

Same as branch service/area names: `mock-provider`, `provider-service`, `compose`, `infra`, …

### Examples

```text
chore(infra): add postgres init for per-service databases
feat(mock-provider): add healthy and server_error chat modes
test(mock-provider): cover timeout and rate_limited modes
feat(provider-service): call mock and normalize completion
fix(inference-service): resume saga when provider outcome exists
chore(compose): add mock-provider to apps profile
docs(readme): document local mock-provider commands
```

### Rules

- Imperative mood: `add`, `fix`, `wire` — not `added` / `fixed`.
- Keep the subject around 72 characters.
- Prefer one logical change per commit.
- Avoid mixing unrelated services in one commit.
- Never commit secrets, `.env`, or real API keys.

### Typical sequence on a service branch

```text
chore(mock-provider): scaffold fastapi project
feat(mock-provider): implement chat completions endpoint
feat(mock-provider): add chaos modes and admin reset
test(mock-provider): add httpx smoke tests
chore(compose): wire mock-provider service
```
