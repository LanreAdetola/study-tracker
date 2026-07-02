# Study Tracker Development Guidelines

## Tech Stack

- **Frontend**: Blazor WebAssembly (.NET 9.0), Bootstrap 5.3.3, Chart.js v4 (JS interop)
- **Backend**: ASP.NET Core Minimal API (.NET 8.0), served from the same container as the client
- **Database**: Azure Cosmos DB (NoSQL), partition key `/userId`
- **Auth**: GitHub/Microsoft OAuth via Azure App Service Authentication ("Easy Auth")
- **Hosting**: Single Docker image (client + API) on Azure App Service (Linux container), built by GitHub Actions and pushed to ghcr.io

## Project Structure

- `client/` — Blazor WASM frontend
- `api/` — ASP.NET Core Minimal API (`Endpoints/` holds route handlers, `Auth/` holds the App Service principal-header parsing)
- `Dockerfile` — multi-stage build: publishes `client/`, publishes `api/`, copies the client's `wwwroot` into the API's static files
- `specs/` — SpecKit feature specifications
- `.specify/` — SpecKit templates, scripts, constitution
- `.claude/skills/` — SpecKit slash commands

## Build & Run

```bash
# Build both projects
dotnet build

# Run locally via Docker (requires a Cosmos DB connection string in .env)
docker compose up --build
```

## Key Patterns

- All API endpoints authenticate via the `x-ms-client-principal-id` header, read through `HttpContext.GetUserId()` (`api/Auth/PrincipalExtensions.cs`); App Service injects the same header names Azure Static Web Apps used to
- The client and API are served from one ASP.NET Core process (same origin) — `api/Program.cs` calls `UseStaticFiles`/`UseBlazorFrameworkFiles` before mapping `/api/*` routes, with `MapFallbackToFile("index.html")` last for SPA routing
- `GET /api/auth/me` reproduces the shape of SWA's old `/.auth/me` response from App Service's raw auth headers — the client's `AppServiceAuthenticationStateProvider` depends on this exact contract
- Session routes use `{id:guid}` constraint to avoid conflicts with named routes like `/stats`
- Chart.js interop is in `client/wwwroot/js/charts.js` — functions: `renderBarChart`, `renderDonutChart`, `renderLineChart`
- Mobile responsiveness uses `d-none d-md-block` / `d-block d-md-none` to switch between table and card layouts
- Touch targets: 44px min-height on mobile via `.btn-mobile` class

## Conventions

- Client models are in `client.Models` namespace
- API models are in `StudyTracker.Api.Models` namespace
- Services follow the same name on client and API (e.g., `StudySessionService`)
- Constitution at `.specify/memory/constitution.md` defines non-negotiable principles

## SpecKit Workflow

Use `/speckit-*` commands for spec-driven development:
1. `/speckit-constitution` — Project principles
2. `/speckit-specify` — Feature specification
3. `/speckit-plan` — Implementation plan
4. `/speckit-tasks` — Task breakdown
5. `/speckit-implement` — Execute tasks

## Active Technologies
- C# — Frontend: .NET 9.0 (Blazor WASM), Backend: .NET 8.0 (Azure Functions isolated) + Blazor WebAssembly, Azure Functions v4, Bootstrap 5, Chart.js v4 (002-goal-completion-dashboard-kpis)

## Recent Changes
- 002-goal-completion-dashboard-kpis: Added C# — Frontend: .NET 9.0 (Blazor WASM), Backend: .NET 8.0 (Azure Functions isolated) + Blazor WebAssembly, Azure Functions v4, Bootstrap 5, Chart.js v4
