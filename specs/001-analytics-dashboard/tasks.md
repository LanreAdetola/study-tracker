# Tasks: Study Analytics Dashboard

**Input**: Design documents from `/specs/001-analytics-dashboard/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in the feature specification. Manual testing via SWA CLI per quickstart.md.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Frontend**: `client/` (Blazor WebAssembly, .NET 9.0)
- **Backend**: `api/` (Azure Functions, .NET 8.0)
- Paths are relative to repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add Chart.js and shared models needed by all user stories.

- [x] T001 Add Chart.js library to `client/wwwroot/lib/chart.js/chart.umd.js` (download from CDN or npm)
- [x] T002 [P] Add Chart.js script reference to `client/wwwroot/index.html`
- [x] T003 [P] Create JS interop file `client/wwwroot/js/charts.js` with chart instance management (create, destroy, resize helpers)
- [x] T004 [P] Create `api/Models/StudySessionStats.cs` with `StudySessionStats` and `DailyHours` classes per data-model.md
- [x] T005 [P] Create `client/Models/AnalyticsModels.cs` with client-side `StudySessionStats`, `DailyHours`, and `GoalProgressPoint` classes per data-model.md

**Checkpoint**: Chart.js is loadable in the browser. Shared models compile on both client and API.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build the stats API endpoint that all frontend user stories depend on.

**CRITICAL**: No frontend chart work can begin until the stats endpoint returns data.

- [x] T006 Add `GetStatsAsync(string userId, DateTime? from, DateTime? to)` method signature to `api/Services/IStudySessionService.cs`
- [x] T007 Implement `GetStatsAsync` in `api/Services/StudySessionService.cs` — query Cosmos DB sessions container filtered by userId partition key and date range, aggregate into `StudySessionStats` (totalSessions, totalHours, averageHoursPerDay, hoursByCategory, dailyBreakdown with zero-fill for missing days)
- [x] T008 Add `GetStudySessionStats` HTTP function to `api/Functions/StudySessionFunctions.cs` — `GET /api/sessions/stats` with optional `from`/`to` query params, authenticate via `x-ms-client-principal-id`, return `StudySessionStats` JSON per contracts/api-stats.md
- [x] T009 Add `GetStatsAsync` method to `client/Services/StudySessionService.cs` — call `GET /api/sessions/stats`, deserialize into client-side `StudySessionStats` model

**Checkpoint**: `curl http://localhost:4280/api/sessions/stats` returns valid JSON with aggregated data for the authenticated user.

---

## Phase 3: User Story 1 — View Daily Study Hours (Priority: P1) MVP

**Goal**: User sees a bar chart of daily study hours for the last 30 days plus summary stats.

**Independent Test**: Log 5+ sessions across different dates, open `/analytics`, verify bar chart renders with correct daily totals and summary stats appear above.

### Implementation for User Story 1

- [x] T010 [US1] Add `renderBarChart(canvasId, labels, data, label)` function to `client/wwwroot/js/charts.js` — renders a Chart.js bar chart with date labels on X-axis and hours on Y-axis
- [x] T011 [US1] Build `client/Pages/Analytics.razor` — page layout with `@page "/analytics"` route, auth check, summary stats section (total sessions, total hours, avg hours/day), and a canvas element for the daily hours bar chart
- [x] T012 [US1] Add code-behind logic in `Analytics.razor` — on init: call `GetStatsAsync`, populate summary stats, call JS interop `renderBarChart` in `OnAfterRenderAsync` with dailyBreakdown data
- [x] T013 [US1] Implement empty state in `Analytics.razor` — when stats show zero sessions, display friendly message with illustration/icon + "No study sessions yet" + link button to `/study-log`
- [x] T014 [US1] Style the Analytics page layout in `client/wwwroot/css/app.css` — summary stat cards, chart container responsive sizing, empty state styling

**Checkpoint**: Analytics page shows summary stats + daily bar chart with real data. Empty state works for users with no sessions.

---

## Phase 4: User Story 2 — View Category Breakdown (Priority: P2)

**Goal**: User sees a donut chart showing proportion of hours per study category.

**Independent Test**: Log sessions in 3+ categories, open `/analytics`, verify donut chart renders with correct proportions and category labels.

### Implementation for User Story 2

- [x] T015 [US2] Add `renderDonutChart(canvasId, labels, data)` function to `client/wwwroot/js/charts.js` — renders a Chart.js donut chart with category labels and proportional segments
- [x] T016 [US2] Add donut chart canvas and section to `client/Pages/Analytics.razor` — positioned below or beside the bar chart, with heading "Study Time by Category"
- [x] T017 [US2] Wire donut chart data in `Analytics.razor` — extract `hoursByCategory` from stats response, call JS interop `renderDonutChart` in `OnAfterRenderAsync`

**Checkpoint**: Analytics page shows both bar chart (US1) and donut chart (US2). Category breakdown matches logged sessions.

---

## Phase 5: User Story 3 — View Goal Progress Over Time (Priority: P3)

**Goal**: User sees a line chart showing cumulative hours toward each active goal over time.

**Independent Test**: Create 2 active goals, log sessions against each, open `/analytics`, verify line chart shows cumulative progress with target reference lines.

### Implementation for User Story 3

- [x] T018 [US3] Add `renderLineChart(canvasId, datasets)` function to `client/wwwroot/js/charts.js` — renders a Chart.js line chart with multiple datasets (one line per goal), supports dashed target reference lines
- [x] T019 [US3] Add goal progress computation logic in `Analytics.razor` — fetch active goals via `StudyGoalService.GetActiveGoalsAsync()`, fetch all sessions, compute `GoalProgressPoint` series (cumulative hours per goal over time by matching session category to goal name)
- [x] T020 [US3] Add line chart canvas and section to `client/Pages/Analytics.razor` — positioned below category chart, with heading "Goal Progress", hidden when user has no active goals
- [x] T021 [US3] Wire line chart in `Analytics.razor` — call JS interop `renderLineChart` in `OnAfterRenderAsync` with goal progress datasets and target hour reference lines

**Checkpoint**: All three chart types render. Goal progress section hides gracefully when no active goals exist.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories.

- [x] T022 [P] Add responsive CSS for Analytics page — charts resize on mobile, summary stats stack vertically on screens < 768px
- [x] T023 [P] Add loading state to Analytics page — show skeleton or spinner while stats API call is in flight
- [x] T024 Add Analytics link to navigation menu in `client/Layout/NavMenu.razor` (if not already present)
- [x] T025 Run quickstart.md validation — verify all scenarios from quickstart.md pass in local SWA CLI environment

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on T004 (API model) — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Phase 1 + Phase 2 completion
- **User Story 2 (Phase 4)**: Depends on Phase 1 + Phase 2 completion (independent from US1 but builds on same page)
- **User Story 3 (Phase 5)**: Depends on Phase 1 + Phase 2 completion (independent from US1/US2 but adds to same page)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **User Story 2 (P2)**: Can start after Phase 2 — adds to Analytics.razor created in US1 but donut chart is an independent section
- **User Story 3 (P3)**: Can start after Phase 2 — adds to Analytics.razor, also needs StudyGoalService (existing)

### Within Each User Story

- JS interop function before Blazor page integration
- Page layout before data wiring
- Core implementation before styling

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T002, T003, T004, T005)
- All Phase 2 tasks are sequential (T006 → T007 → T008 → T009)
- Within US1: T010 (JS) can run in parallel with T014 (CSS)
- Polish tasks marked [P] can run in parallel (T022, T023)

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (Chart.js + models)
2. Complete Phase 2: Foundational (stats API endpoint)
3. Complete Phase 3: User Story 1 (bar chart + summary stats + empty state)
4. **STOP and VALIDATE**: Test with real data via SWA CLI
5. Deploy to production — Analytics page is usable with just the daily chart

### Incremental Delivery

1. Complete Setup + Foundational → API ready
2. Add User Story 1 → Test → Deploy (MVP: daily chart + summary stats)
3. Add User Story 2 → Test → Deploy (adds category donut chart)
4. Add User Story 3 → Test → Deploy (adds goal progress line chart)
5. Polish → Test → Deploy (responsive, loading states, nav link)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story adds a chart type to the same Analytics.razor page
- Chart.js instances must be destroyed before re-creation (handled in charts.js helpers)
- Commit after each task or logical group
- Stop at any checkpoint to validate independently
