# Study Tracker

![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4?logo=blazor&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-9.0%20%7C%208.0-512BD4?logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-App%20Service-2496ED?logo=docker&logoColor=white)
![Cosmos DB](https://img.shields.io/badge/Cosmos%20DB-NoSQL-0078D4?logo=microsoftazure&logoColor=white)

> A full-stack study tracking app built on Azure, designed to help users log sessions, set goals, and visualize their progress over time.

## What it does

Study Tracker is a web application that lets users log study sessions, create goals for certifications and subjects, and see how their study habits look over time through charts and KPIs. The idea came from wanting a simple, focused tool to track certification study hours without the overhead of a full project management app.

Users authenticate with GitHub or Microsoft via Azure App Service's built-in auth ("Easy Auth"). Once logged in, they can log study sessions with a category, hours, date, and optional notes. They can create up to 5 goals (e.g., "AI-300 Machine Learning Operations — 25 hours") and track cumulative progress against each one. The analytics page shows daily study hours as a bar chart, category breakdown as a donut chart, and goal progress as a line chart with target reference lines — all powered by Chart.js via Blazor JS interop.

The dashboard shows four KPI cards at a glance: total sessions, total hours, current study streak (consecutive days), and a weekly comparison with a trend arrow. Completed goals get a green badge and a congratulatory toast notification the moment you log the session that pushes past the target. The whole app is mobile-optimized with a slide-out nav drawer, compact session lists, tab navigation on the goals page, and touch-friendly tap targets.

## Architecture

```text
Browser (Blazor WebAssembly)
    |
    |-- Client-side SPA (.NET 9.0)
    |   Handles routing, UI rendering, Chart.js interop
    |
    v
Azure App Service (single Linux container)
    |-- ASP.NET Core Minimal API (.NET 8.0) serves the Blazor WASM
    |   static files and the /api/* endpoints from one process
    |-- App Service Authentication ("Easy Auth") handles GitHub/
    |   Microsoft OAuth and injects x-ms-client-principal headers
    |-- REST API: sessions CRUD, goals CRUD, user registration
    |-- Stats endpoint: aggregates sessions into daily breakdown,
    |   category hours, streaks, and weekly comparison
    |
    v
Azure Cosmos DB (NoSQL)
    |-- Partition key: /userId (single-partition reads, per-user isolation)
    |-- Containers: sessions, goals, users
```

Every API request is scoped to the authenticated user's partition key, so there is zero chance of cross-user data leakage. The stats endpoint computes streaks and weekly comparisons server-side to avoid sending raw session data to the browser.

## Azure services used

- **Azure App Service (Linux container)** — Runs a single Docker image containing both the Blazor WASM static files and the ASP.NET Core Minimal API, pulled from ghcr.io. Handles OAuth authentication (GitHub + Microsoft) via built-in App Service Authentication
- **Azure Cosmos DB** — NoSQL document database with `/userId` as the partition key. Stores sessions, goals, and user profiles. All queries are single-partition reads for performance and cost efficiency
- **Application Insights** — Monitoring and telemetry for the API

## Deployment

The app deploys through a GitHub Actions workflow (`.github/workflows/docker-build-deploy.yml`) that triggers on push to `main`:

1. **Build and push** — Builds a single multi-stage Docker image (Blazor client + ASP.NET Core API) and pushes it to `ghcr.io` tagged with both the commit SHA and `latest`.
2. **Deploy** — Points the Azure App Service container slot at the newly pushed image and restarts the app.

Authentication with Azure uses **OIDC token exchange** — no long-lived Azure credentials stored in GitHub Secrets. Pushing to `ghcr.io` uses the built-in `GITHUB_TOKEN`.

## How to run locally

```bash
git clone https://github.com/LanreAdetola/study-tracker.git
cd study-tracker

# Provide your Cosmos DB connection string
echo "CosmosDBConnectionString=<your-cosmos-db-connection-string>" > .env

docker compose up --build
```

The app is served at `http://localhost:8080`. App Service Authentication doesn't run locally — simulate a logged-in user by manually sending an `X-MS-CLIENT-PRINCIPAL-ID` header (e.g. via curl or a browser extension) when testing authenticated endpoints.

Prerequisites: [Docker](https://www.docker.com/) (or [.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) + [.NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) SDKs to run `client/` and `api/` directly without containers).

## What I learned

The biggest surprise was how much Azure Static Web Apps handles for you — and how specific you need to be with the parts it doesn't. Authentication "just works" by adding `/.auth/login/github` links, and the `x-ms-client-principal-id` header appears on every API request without any middleware. But the SPA fallback routing caught me off guard: `staticwebapp.config.json` has to live inside `client/wwwroot/` so it ends up in the deployed output. I had it in the repo root for weeks and couldn't figure out why page refreshes on `/goals` or `/analytics` returned Azure's 404 page. Once I moved it, every route worked instantly.

Chart.js via Blazor JS interop turned out to be simpler than I expected. Instead of pulling in a Blazor wrapper library, I wrote a single `charts.js` file with three functions (`renderBarChart`, `renderDonutChart`, `renderLineChart`) and called them from `OnAfterRenderAsync`. The tricky part was getting the goal progress chart to work when users backfilled sessions — each goal had its own date range, and Chart.js was treating the X-axis as categorical instead of chronological. The fix was building a shared sorted timeline across all goals and using `null` values for gaps.

If I were starting over, I'd add the toast notification system earlier — it's useful for every CRUD operation, not just goal completion. I'd also set up the `staticwebapp.config.json` in `wwwroot/` from day one and use the Cosmos DB emulator locally instead of connecting to the live database. The 50-user cap was a deliberate free-tier constraint, but it turned out to be a useful design forcing function: it made me think about per-user data isolation from the start rather than bolting it on later.

## Author

Built by [Lanre Adetola](https://github.com/LanreAdetola)
