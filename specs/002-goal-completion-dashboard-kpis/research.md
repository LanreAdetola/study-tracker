# Research: Goal Completion & Dashboard KPIs

**Date**: 2026-04-03
**Feature**: 002-goal-completion-dashboard-kpis

## R1: Streak Calculation Approach

### Decision

Compute streaks **server-side** in the existing `GetStatsAsync` method by
querying all session dates for the user, grouping by date, and counting
consecutive days backward from today (current streak) and scanning all
dates for the longest run (longest streak).

### Rationale

- Streak requires all-time session data, not just the 30-day window.
  The stats method already queries sessions — extend the query to fetch
  all dates (lightweight: just date field, not full documents).
- Server-side avoids sending all sessions to the client.
- Cosmos DB single-partition query on userId is fast even for all-time.

### Alternatives Considered

- **Client-side calculation**: Fetch all sessions and compute in Blazor.
  Works at current scale but wasteful — sends full session payloads
  just to extract dates.
- **Separate streak endpoint**: Adds a new API route. Unnecessary
  complexity per Simplicity First principle.
- **Cached streak field on user profile**: Would require updating on
  every session CRUD. Over-engineering for 50 users.

## R2: Weekly Comparison Logic

### Decision

Compute `thisWeekHours` and `lastWeekHours` server-side in `GetStatsAsync`.
Week boundaries follow ISO standard: Monday 00:00 to Sunday 23:59.

### Rationale

- Simple date math, no external libraries needed.
- Consistent with how users think about "this week" in a study context.
- The trend arrow (up/down/equal) is derived client-side from the two
  numbers — no need to compute it server-side.

### Pattern

```
thisWeekStart = today - (today.DayOfWeek adjusted to Monday)
lastWeekStart = thisWeekStart - 7 days
lastWeekEnd = thisWeekStart - 1 day

thisWeekHours = SUM(hours) WHERE date >= thisWeekStart
lastWeekHours = SUM(hours) WHERE date >= lastWeekStart AND date < thisWeekStart
```

## R3: Toast Component Pattern for Blazor

### Decision

Build a simple **cascading parameter toast service** pattern:
1. `ToastService` — injectable service with an `OnShow` event
2. `Toast.razor` — component in MainLayout that listens to the service
3. Any page can inject `ToastService` and call `Show(message, type)`

### Rationale

- Standard Blazor pattern for cross-component communication.
- No JS interop needed — pure Blazor with CSS animation.
- Bootstrap 5 toast markup can be used but rendered by Blazor, not
  Bootstrap's JS.
- Auto-dismiss via `Task.Delay(5000)` + `StateHasChanged()`.

### Alternatives Considered

- **JS interop with Bootstrap toast**: Requires calling Bootstrap's JS
  API. Adds JS dependency for a simple component.
- **Third-party Blazor toast library** (e.g., Blazored.Toast): External
  NuGet dependency for a feature we can build in ~50 lines.
- **Inline alerts**: Already used for errors. Not suitable for
  transient success messages — they require manual dismissal.

## R4: Goal Completion Detection

### Decision

Detect completion **client-side** by comparing `goal.CurrentHours >= goal.TargetHours`.
For the toast trigger, compare before/after state when a session is saved:
capture goal hours before the save, then re-fetch goals after, and toast
for any goal that crossed the threshold.

### Rationale

- No backend change needed for completion detection — it's a pure
  client-side UI concern.
- The "before/after" comparison ensures the toast only fires on the
  transition, not on every page load for completed goals.
- This logic lives in the StudyLog page (where sessions are created),
  not in the Goals page.
