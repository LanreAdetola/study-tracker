# Research: Study Analytics Dashboard

**Date**: 2026-04-03
**Feature**: 001-analytics-dashboard

## R1: Charting Library for Blazor WebAssembly

### Decision

Use **Chart.js v4** via JavaScript interop.

### Rationale

- Chart.js is lightweight (~70KB minified), well-documented, and widely used.
- Blazor WASM supports JS interop natively — no wrapper library needed.
- The project already uses JS interop patterns (Bootstrap modals).
- Chart.js provides bar, donut, and line chart types — all three needed.
- No additional NuGet packages required, keeping dependencies minimal
  (Constitution: Simplicity First).

### Alternatives Considered

- **Radzen Blazor Charts**: Full Blazor component library. Adds a large
  dependency for just 3 charts. Rejected per Simplicity First principle.
- **ApexCharts.Blazor**: NuGet wrapper around ApexCharts. Adds two
  dependencies (NuGet + JS). More abstraction than needed.
- **ChartJs.Blazor**: Community Blazor wrapper for Chart.js. Last updated
  2023, targets older Chart.js versions. Maintenance risk.
- **Server-side SVG rendering**: No JS dependency but significantly more
  complex to build. No interactivity (tooltips, hover).

## R2: Server-Side Aggregation vs. Client-Side

### Decision

Use a **new server-side stats endpoint** (`GET /api/sessions/stats`) for
summary statistics and daily breakdown. Client fetches pre-aggregated data.

### Rationale

- Avoids sending all raw sessions to the client just for aggregation.
- Cosmos DB can perform aggregation queries efficiently within a single
  partition (userId).
- Reduces client-side complexity — the Blazor page just renders what the
  API returns.
- Aligns with the user's input specifying a `GET /api/analytics/summary`
  endpoint approach.

### Alternatives Considered

- **Client-side aggregation only**: Fetch all sessions via existing
  `GET /api/sessions`, aggregate in Blazor. Works at current scale but
  sends unnecessary data over the wire and pushes compute to the browser.
  Rejected for scalability and separation of concerns.
- **Hybrid**: Use existing endpoint for goal progress (needs raw session
  dates), new endpoint for summaries. Adds complexity without clear benefit
  at current scale.

## R3: Chart.js Integration Pattern

### Decision

Use a single **charts.js** interop file with named functions per chart type.
Blazor calls these via `IJSRuntime.InvokeVoidAsync`.

### Rationale

- One JS file is simpler to maintain than one per chart.
- Named functions (`renderBarChart`, `renderDonutChart`, `renderLineChart`)
  are explicit and easy to call from Blazor.
- Chart instances are tracked by canvas ID so they can be destroyed and
  re-created when data changes.

### Pattern

```
Blazor (Analytics.razor)
  → OnAfterRenderAsync
    → IJSRuntime.InvokeVoidAsync("renderBarChart", canvasId, labels, data)
    → IJSRuntime.InvokeVoidAsync("renderDonutChart", canvasId, labels, data)
    → IJSRuntime.InvokeVoidAsync("renderLineChart", canvasId, datasets)
```

## R4: Cosmos DB Aggregation Query Performance

### Decision

Use a single Cosmos DB query with GROUP BY for daily aggregation, scoped to
the userId partition key. Limit to last 30 days with a date filter.

### Rationale

- Single-partition queries with GROUP BY are efficient and consume minimal
  RUs on Cosmos DB.
- The 30-day window keeps the result set small (max 30 rows for daily
  breakdown).
- Category aggregation is a simple GROUP BY on the category field.

### Query Pattern

```sql
SELECT c.date, SUM(c.hours) as totalHours
FROM c
WHERE c.userId = @userId AND c.date >= @startDate
GROUP BY c.date
```

Note: Cosmos DB NoSQL API supports GROUP BY with aggregate functions. If
GROUP BY proves problematic, fallback to fetching filtered sessions and
aggregating in the Azure Function (still server-side, still fast for <1000
sessions per user).
