# Study Tracker

![Blazor](https://img.shields.io/badge/Blazor-WASM-512BD4?logo=blazor&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-9.0%20%7C%208.0-512BD4?logo=dotnet&logoColor=white)
![Azure Static Web Apps](https://img.shields.io/badge/Azure-Static%20Web%20Apps-0078D4?logo=microsoftazure&logoColor=white)
![Cosmos DB](https://img.shields.io/badge/Cosmos%20DB-NoSQL-0078D4?logo=microsoftazure&logoColor=white)

> A full-stack study tracking app built on Azure, designed to help users log sessions, set goals, and visualize their progress over time.

**Live app:** [https://kind-sky-0c0b6ea03.3.azurestaticapps.net/](https://kind-sky-0c0b6ea03.3.azurestaticapps.net/)

## What it does

Study Tracker is a web application that lets users log study sessions, create goals for certifications and subjects, and see how their study habits look over time through charts and KPIs. The idea came from wanting a simple, focused tool to track certification study hours without the overhead of a full project management app.

Users authenticate with GitHub or Microsoft via Azure Static Web Apps' built-in auth. Once logged in, they can log study sessions with a category, hours, date, and optional notes. They can create up to 5 goals (e.g., "AI-300 Machine Learning Operations — 25 hours") and track cumulative progress against each one. The analytics page shows daily study hours as a bar chart, category breakdown as a donut chart, and goal progress as a line chart with target reference lines — all powered by Chart.js via Blazor JS interop.

The dashboard shows four KPI cards at a glance: total sessions, total hours, current study streak (consecutive days), and a weekly comparison with a trend arrow. Completed goals get a green badge and a congratulatory toast notification the moment you log the session that pushes past the target. The whole app is mobile-optimized with a slide-out nav drawer, compact session lists, tab navigation on the goals page, and touch-friendly tap targets.

## Architecture

```text
Browser (Blazor WebAssembly)
    |
    |-- Client-side SPA (.NET 9.0)
    |   Handles routing, UI rendering, Chart.js interop
    |
    v
Azure Static Web Apps
    |-- Serves the Blazor WASM static files
    |-- Handles GitHub/Microsoft OAuth (built-in auth provider)
    |-- Routes /api/* to the managed Azure Functions backend
    |-- Creates staging environments for pull requests
    |
    v
Azure Functions v4 (isolated worker, .NET 8.0)
    |-- REST API: sessions CRUD, goals CRUD, user registration
    |-- Stats endpoint: aggregates sessions into daily breakdown,
    |   category hours, streaks, and weekly comparison
    |-- Authenticates via x-ms-client-principal-id header from SWA
    |
    v
Azure Cosmos DB (NoSQL)
    |-- Partition key: /userId (single-partition reads, per-user isolation)
    |-- Containers: sessions, goals, users
```

Every API request is scoped to the authenticated user's partition key, so there is zero chance of cross-user data leakage. The stats endpoint computes streaks and weekly comparisons server-side to avoid sending raw session data to the browser.

## Azure services used

- **Azure Static Web Apps** — Hosts the Blazor WASM frontend, manages the Azure Functions API, handles OAuth authentication (GitHub + Microsoft), and automatically provisions staging environments for pull requests
- **Azure Functions v4** — Serverless API layer running .NET 8.0 in isolated worker mode. Handles all CRUD operations and server-side analytics aggregation
- **Azure Cosmos DB** — NoSQL document database with `/userId` as the partition key. Stores sessions, goals, and user profiles. All queries are single-partition reads for performance and cost efficiency
- **Application Insights** — Monitoring and telemetry for the Azure Functions backend

## Deployment

The app deploys automatically through a GitHub Actions workflow (`.github/workflows/azure-static-web-apps-kind-sky-0c0b6ea03.yml`). The workflow triggers on:

- **Push to `main`** — Builds and deploys the Blazor frontend and Azure Functions API to production
- **Pull request events** (opened, synchronize, reopened) — Deploys to an isolated staging environment for testing
- **Pull request closed** — Automatically tears down the staging environment

Authentication with Azure uses **OIDC token exchange** — no long-lived credentials stored in GitHub Secrets. The workflow requests an ID token at runtime, which is passed to the `Azure/static-web-apps-deploy@v1` action. The only secret required is `AZURE_STATIC_WEB_APPS_API_TOKEN_KIND_SKY_0C0B6EA03`, which authorizes the deployment.

The SWA deploy action handles the build internally: it compiles the Blazor WASM project from `client/`, the Azure Functions project from `api/`, and deploys both as a single unit.

## How to run locally

```bash
git clone https://github.com/LanreAdetola/study-tracker.git
cd study-tracker

# Create API local settings with your Cosmos DB connection
cat > api/local.settings.json << EOF
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDBConnectionString": "<your-cosmos-db-connection-string>"
  }
}
EOF

# Run both frontend and API together
swa start http://localhost:5000 --api-location api
```

Prerequisites: [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0), [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local), and the [SWA CLI](https://github.com/Azure/static-web-apps-cli).

## What I learned

The biggest surprise was how much Azure Static Web Apps handles for you — and how specific you need to be with the parts it doesn't. Authentication "just works" by adding `/.auth/login/github` links, and the `x-ms-client-principal-id` header appears on every API request without any middleware. But the SPA fallback routing caught me off guard: `staticwebapp.config.json` has to live inside `client/wwwroot/` so it ends up in the deployed output. I had it in the repo root for weeks and couldn't figure out why page refreshes on `/goals` or `/analytics` returned Azure's 404 page. Once I moved it, every route worked instantly.

Chart.js via Blazor JS interop turned out to be simpler than I expected. Instead of pulling in a Blazor wrapper library, I wrote a single `charts.js` file with three functions (`renderBarChart`, `renderDonutChart`, `renderLineChart`) and called them from `OnAfterRenderAsync`. The tricky part was getting the goal progress chart to work when users backfilled sessions — each goal had its own date range, and Chart.js was treating the X-axis as categorical instead of chronological. The fix was building a shared sorted timeline across all goals and using `null` values for gaps.

If I were starting over, I'd add the toast notification system earlier — it's useful for every CRUD operation, not just goal completion. I'd also set up the `staticwebapp.config.json` in `wwwroot/` from day one and use the Cosmos DB emulator locally instead of connecting to the live database. The 50-user cap was a deliberate free-tier constraint, but it turned out to be a useful design forcing function: it made me think about per-user data isolation from the start rather than bolting it on later.

## Author

Built by [Lanre Adetola](https://github.com/LanreAdetola)
