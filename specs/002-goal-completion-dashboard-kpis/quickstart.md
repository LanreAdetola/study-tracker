# Quickstart: Goal Completion & Dashboard KPIs

**Feature**: 002-goal-completion-dashboard-kpis

## Prerequisites

- .NET 9.0 SDK (frontend), .NET 8.0 SDK (API)
- Azure Functions Core Tools v4, SWA CLI
- Cosmos DB connection in `api/local.settings.json`
- At least a few study sessions and goals for the test user

## Running Locally

```bash
swa start http://localhost:5000 --api-location api
```

## Verifying the Feature

### Dashboard KPIs

1. **Log in** and navigate to the Dashboard (`/`).
2. **Verify streak**: Check that "Current Streak" shows consecutive
   days studied. Log a session for today if streak shows 0.
3. **Verify weekly comparison**: Check "This Week" vs "Last Week"
   hours. The trend arrow should point up if this week > last week.
4. **Verify goal progress**: Check that up to 3 active goals appear
   with progress bars. Click a goal to navigate to the Goals page.
5. **Test empty state**: Create a new user with no sessions — all
   KPIs should show 0 with appropriate messaging.

### Goal Completion Badge

1. **Create a goal** with a low target (e.g., 2 hours).
2. **Log sessions** totaling 2+ hours against that goal.
3. **Navigate to Goals page** — the goal should show a green
   "Completed" badge and green progress bar.
4. **Increase the target** to 10 hours — the badge should disappear.

### Toast Notification

1. **Create a goal** with a 1-hour target.
2. **Log a 1-hour session** for that goal.
3. **Verify toast**: A success toast should appear at the top-right
   saying the goal is completed. It should auto-dismiss after 5s.
4. **Log another session** for the same goal — no toast should appear
   (already completed).

### Stats API

```bash
curl http://localhost:4280/api/sessions/stats
```

Verify response includes `currentStreak`, `longestStreak`,
`thisWeekHours`, `lastWeekHours` fields.

## Key Files

| File | Purpose |
| ---- | ------- |
| `client/Pages/Dashboard.razor` | Enhanced KPI dashboard |
| `client/Pages/Goals.razor` | Completion badge logic |
| `client/Components/Toast.razor` | Reusable toast component |
| `client/Layout/MainLayout.razor` | Toast component placement |
| `client/Models/AnalyticsModels.cs` | Extended stats model |
| `api/Models/StudySessionStats.cs` | Extended stats response |
| `api/Services/StudySessionService.cs` | Streak + weekly calculation |
