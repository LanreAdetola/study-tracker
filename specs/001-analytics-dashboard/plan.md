# Implementation Plan: Study Analytics Dashboard

**Branch**: `001-analytics-dashboard` | **Date**: 2026-04-03 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-analytics-dashboard/spec.md`

## Summary

Build a Study Analytics Dashboard that visualizes study habits through three
chart types: daily study hours (bar chart, last 30 days), category breakdown
(donut chart), and goal progress over time (line chart). The backend adds a
new `GET /api/sessions/stats` endpoint that aggregates session data server-side.
The frontend uses Chart.js via JS interop for rendering. A designed empty state
guides users with no data.

## Technical Context

**Language/Version**: C# — Frontend: .NET 9.0 (Blazor WASM), Backend: .NET 8.0 (Azure Functions isolated)
**Primary Dependencies**: Blazor WebAssembly, Azure Functions v4, Chart.js (new — via JS interop), Bootstrap 5
**Storage**: Azure Cosmos DB (NoSQL), partition key `/userId`
**Testing**: Manual testing via SWA CLI local environment
**Target Platform**: Web (Blazor WebAssembly), hosted on Azure Static Web Apps
**Project Type**: Full-stack web application (SPA + serverless API)
**Performance Goals**: Analytics page loads in under 2 seconds
**Constraints**: Max 50 users, free-tier Cosmos DB, single-partition queries only
**Scale/Scope**: ~50 users, moderate session counts per user

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle                | Status | Notes |
| ------------------------ | ------ | ----- |
| I. Simplicity First | PASS | Chart.js is the simplest viable charting solution — one JS file, no Blazor wrapper library. Server-side aggregation keeps the frontend logic minimal. |
| II. Data Integrity | PASS | Stats endpoint validates data server-side (excludes future dates, non-positive hours). Client displays only what the API returns. |
| III. User Isolation | PASS | Stats endpoint scopes all queries to authenticated userId via `x-ms-client-principal-id` header. Cosmos queries use userId partition key. |
| IV. Incremental Development | PASS | Three user stories at P1/P2/P3 are independently deliverable. New endpoint is additive — no changes to existing APIs. |
| V. Observability | PASS | Stats endpoint logs aggregation requests via Application Insights (existing infrastructure). |

**Gate result: PASS** — no violations.

## Project Structure

### Documentation (this feature)

```text
specs/001-analytics-dashboard/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api-stats.md
└── tasks.md             # Phase 2 output (/speckit-tasks)
```

### Source Code (repository root)

```text
client/
├── Pages/
│   └── Analytics.razor          # Analytics page (currently empty)
├── Models/
│   └── AnalyticsModels.cs       # New: response models for stats endpoint
├── Services/
│   └── StudySessionService.cs   # Extended: add GetStatsAsync method
└── wwwroot/
    ├── js/
    │   └── charts.js            # New: Chart.js interop functions
    └── lib/
        └── chart.js/
            └── chart.umd.js     # New: Chart.js library (CDN or local)

api/
├── Functions/
│   └── StudySessionFunctions.cs # Extended: add GetStudySessionStats endpoint
├── Services/
│   ├── IStudySessionService.cs  # Extended: add GetStatsAsync method
│   └── StudySessionService.cs   # Extended: implement stats aggregation query
└── Models/
    └── StudySessionStats.cs     # New: stats response model
```

**Structure Decision**: Extends the existing `client/` + `api/` web application
structure. No new projects or directories beyond what's needed for Chart.js
assets and the analytics models.

## Complexity Tracking

> No constitution violations to justify.

No entries.
