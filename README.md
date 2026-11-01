# Evently

Modular monolith for event management and ticketing (.NET, PostgreSQL, Keycloak, RabbitMQ/MassTransit, Redis, Seq, Jaeger), with Users, Events, Ticketing and Attendance modules. API testing is done with a Postman collection.

## Course tasks (to-do.txt)

| ID | Task | Points | Notes |
|----|------|:------:|-------|
| C3 | CI/CD | 2 | GitHub Actions CI (`.github/workflows/ci.yml`): build + unit/architecture/auth tests + integration tests on push/PR to `main`. No auto-deploy (deploy excluded) |
| C4 | Containerization | 1 | Dockerfile + docker-compose |
| C5 | REST API | 3 | Minimal API endpoints in every module |
| QA1 | Web API testing | 2 | Postman collection |
| B1 | Pagination API | 2 | `GET events/search?skip=&limit=` returns `skip`, `limit`, `totalCount`, `nextLink`; limit validated 1-100 |
| C16 | OAuth2 / authN / authZ | 3 | Keycloak (OIDC/JWT) with permission-based authorization |
| B6 | Feature flags | 3 | Microsoft.FeatureManagement + custom `Plan` filter; `PremiumPage` flag, Member/Premium plans, `PUT users/profile/plan`, `GET users/premium`, `GET users/profile/features` |
| QA3 | AuthN/AuthZ tests | 3 | `test/Evently.AuthTests` (14 xUnit tests) + `postman/Evently.Auth.postman_collection.json` (401/403/200, paging, plan flow) |
| C17 | Favourite project | 6 | Open-source GitHub repo |
| B8 | Async process (queues) | 4 | RabbitMQ via MassTransit, outbox/inbox, cancel-event saga |
| B9 | API caching | 3 | Redis (`ICacheService`) caches `events/search` results for 5 min; invalidated when an event is published, canceled or rescheduled. Carts also use Redis |
| C22 | MCP server | 4 | MCP server at `/mcp` (Streamable HTTP, JWT-protected): tools `search_events`, `get_event`, `list_categories` (read) and `create_category` (write) |

## Total

**36 points** (C3 2 + C4 1 + C5 3 + QA1 2 + B1 2 + C16 3 + B6 3 + QA3 3 + C17 6 + B8 4 + B9 3 + C22 4)

## New features

### Pagination (B1)
`GET /events/search?skip=0&limit=15[&categoryId=&startDate=&endDate=]` returns
`{ skip, limit, totalCount, nextLink, events }`. `nextLink` keeps all other query parameters and is `null` on the last page.

### API caching (B9)
`events/search` results are cached in Redis for 5 minutes, keyed by category, dates, `skip` and `limit`. Publishing, canceling or rescheduling an event bumps a cache version key, so stale results are never served after a change (old entries expire on their own). Tests: `SearchEventsTests` in `Evently.Modules.Events.IntegrationTests`.

### Feature flags and plans (B6)
- Plans are the `Member` (default) and `Premium` roles; `Premium` additionally has the `premium:access` permission.
- `PUT /users/profile/plan` with `{ "plan": "Premium" | "Member" }` switches the caller's plan (demo upgrade, no billing).
- The `PremiumPage` feature flag (see `FeatureManagement` in `appsettings.json`) uses the custom `Plan` filter, which enables it only for users holding `premium:access`.
- `GET /users/premium` is the premium-only page (403 otherwise); `GET /users/profile/features` shows the caller's flags.
- Requires the new `Add_Premium_Plan` migration (applied automatically in Development).

### Auth tests (QA3)
- `dotnet test test/Evently.AuthTests` - permission handler, policy provider, claims helpers, plan filter, plan switching.
- Postman: import `postman/Evently.Auth.postman_collection.json` and run it in order with the stack up (`baseUrl` 5000, `keycloakUrl` 18080). It registers a fresh member, then checks 401/403/200, paging and the plan flow.

### MCP server (C22)
Endpoint: `POST/GET /mcp` (Streamable HTTP), requires a Bearer token from Keycloak.

| Tool | Type | Description |
|------|------|-------------|
| `search_events` | read | Filter by category/dates, `skip`/`limit` |
| `get_event` | read | Event with ticket types |
| `list_categories` | read | All categories |
| `create_category` | write | Needs `categories:update` (Administrator) |

Example with Claude Code:
```
claude mcp add --transport http evently http://localhost:5000/mcp --header "Authorization: Bearer <token>"
```
Token: `POST http://localhost:18080/realms/evently/protocol/openid-connect/token` (`grant_type=password`, `client_id=evently-public-client`).

### CI (C3, tests only)
`.github/workflows/ci.yml` builds the solution and runs unit, architecture and auth tests, then integration tests (Testcontainers) on every push/PR to `main`.

## Remaining easy-ish options
C9 (+1) analytics, C2 (+2) / full C3 deploy (cloud hosting), B4 (+3) search engine.
