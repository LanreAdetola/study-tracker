# Quickstart: Study Analytics Dashboard

**Feature**: 001-analytics-dashboard

## Prerequisites

- .NET 9.0 SDK (frontend)
- .NET 8.0 SDK (API — Azure Functions)
- Azure Functions Core Tools v4
- Azure Static Web Apps CLI (`swa`)
- A running Cosmos DB instance with connection string in `api/local.settings.json`
- At least a few study sessions logged for the test user

## Running Locally

```bash
# From the repo root
swa start http://localhost:5000 --api-location api
```

Or run frontend and API separately:

```bash
# Terminal 1: API
cd api && func start

# Terminal 2: Frontend
cd client && dotnet run
```

## Verifying the Feature

1. **Log in** via GitHub OAuth (or use SWA CLI auth emulation).
2. **Log a few study sessions** on the Study Log page across different dates
   and categories (if you haven't already).
3. **Navigate to Analytics** (`/analytics`) from the nav menu.
4. **Verify**:
   - Summary stats appear at the top (total sessions, total hours, avg/day).
   - Bar chart shows daily hours for the last 30 days.
   - Donut chart shows category breakdown.
   - If you have active goals with linked sessions, a line chart shows goal
     progress over time.
5. **Test empty state**: Create a new user (or clear sessions) and verify the
   Analytics page shows a friendly empty state with a link to Study Log.

## Testing the Stats API Directly

```bash
# With SWA CLI running, the auth header is emulated
curl http://localhost:4280/api/sessions/stats

# With date range
curl "http://localhost:4280/api/sessions/stats?from=2026-03-01&to=2026-04-03"
```

## Key Files

| File | Purpose |
| ---- | ------- |
| `client/Pages/Analytics.razor` | Analytics page with charts |
| `client/Models/AnalyticsModels.cs` | Client-side response models |
| `client/Services/StudySessionService.cs` | Extended with GetStatsAsync |
| `client/wwwroot/js/charts.js` | Chart.js interop functions |
| `api/Functions/StudySessionFunctions.cs` | Extended with stats endpoint |
| `api/Services/StudySessionService.cs` | Extended with stats query |
| `api/Models/StudySessionStats.cs` | Stats response model |
