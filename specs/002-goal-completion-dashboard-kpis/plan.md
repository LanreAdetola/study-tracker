# Implementation Plan: Goal Completion & Dashboard KPIs

**Branch**: `002-goal-completion-dashboard-kpis` | **Date**: 2026-04-03 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-goal-completion-dashboard-kpis/spec.md`

## Summary

Enrich the authenticated dashboard with study streak (current + longest),
weekly hour comparison with trend arrow, and top 3 active goal progress bars.
Add a "Completed" badge and green progress bar to goals that reach 100%.
Build a reusable toast notification component and trigger it on goal completion.
The backend stats endpoint is extended with streak and weekly comparison fields.

## Technical Context

**Language/Version**: C# — Frontend: .NET 9.0 (Blazor WASM), Backend: .NET 8.0 (Azure Functions isolated)
**Primary Dependencies**: Blazor WebAssembly, Azure Functions v4, Bootstrap 5, Chart.js v4
**Storage**: Azure Cosmos DB (NoSQL), partition key `/userId`
**Testing**: Manual testing via SWA CLI local environment
**Target Platform**: Web (Blazor WebAssembly), hosted on Azure Static Web Apps
**Project Type**: Full-stack web application (SPA + serverless API)
**Performance Goals**: Dashboard loads all KPIs in under 2 seconds
**Constraints**: Max 50 users, free-tier Cosmos DB, single-partition queries only
**Scale/Scope**: ~50 users, moderate session counts per user

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle                | Status | Notes |
| ------------------------ | ------ | ----- |
| I. Simplicity First | PASS | Extends existing stats endpoint — no new endpoints. Toast is a single reusable component. Completion badge is a CSS conditional — no schema changes. |
| II. Data Integrity | PASS | Streak and weekly data computed server-side from validated session records. No new data persisted. |
| III. User Isolation | PASS | All stats scoped to authenticated userId via partition key. |
| IV. Incremental Development | PASS | Three independent user stories (P1/P2/P3). Dashboard KPIs, goal badge, and toast can ship separately. |
| V. Observability | PASS | Stats computation logged via existing Application Insights. |

**Gate result: PASS** — no violations.

## Project Structure

### Documentation (this feature)

```text
specs/002-goal-completion-dashboard-kpis/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api-stats-v2.md
└── tasks.md             # Phase 2 output (/speckit-tasks)
```

### Source Code (repository root)

```text
client/
├── Pages/
│   ├── Dashboard.razor          # Rewritten: KPI cards, streak, weekly, goals
│   └── Goals.razor              # Extended: completion badge + green bar
├── Components/
│   └── Toast.razor              # New: reusable toast notification component
├── Models/
│   └── AnalyticsModels.cs       # Extended: add streak + weekly fields
├── Services/
│   └── StudySessionService.cs   # Existing GetStatsAsync (no change)
└── Layout/
    └── MainLayout.razor         # Extended: include Toast component

api/
├── Models/
│   └── StudySessionStats.cs     # Extended: add streak + weekly fields
└── Services/
    └── StudySessionService.cs   # Extended: streak + weekly calculation
```

**Structure Decision**: Extends the existing `client/` + `api/` web application
structure. One new component (Toast.razor). No new endpoints — extends the
existing stats response.

## Complexity Tracking

> No constitution violations to justify.

No entries.
