# Tasks: Goal Completion & Dashboard KPIs

**Input**: Design documents from `/specs/002-goal-completion-dashboard-kpis/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested. Manual testing via SWA CLI per quickstart.md.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

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

**Purpose**: Extend shared models that multiple user stories depend on.

- [x] T001 [P] Add `CurrentStreak`, `LongestStreak`, `ThisWeekHours`, `LastWeekHours` properties to `api/Models/StudySessionStats.cs`
- [x] T002 [P] Add `CurrentStreak`, `LongestStreak`, `ThisWeekHours`, `LastWeekHours` properties to `client/Models/AnalyticsModels.cs` (client-side `StudySessionStats` class)

**Checkpoint**: Both API and client stats models compile with the new fields.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement server-side streak and weekly calculation that US1 dashboard depends on.

**CRITICAL**: Dashboard KPIs cannot display streak/weekly data until the API returns it.

- [x] T003 Extend `GetStatsAsync` in `api/Services/StudySessionService.cs` — after the existing aggregation logic, query ALL user sessions (not just 30-day window) to get distinct dates, compute current streak (consecutive days backward from today/yesterday), longest streak (scan all dates), this week hours (Monday-Sunday), and last week hours. Populate the 4 new fields on `StudySessionStats`.
- [x] T004 Verify the stats endpoint returns new fields — build the API project and confirm no compilation errors in `api/Functions/StudySessionFunctions.cs`

**Checkpoint**: `GET /api/sessions/stats` returns `currentStreak`, `longestStreak`, `thisWeekHours`, `lastWeekHours` with correct values.

---

## Phase 3: User Story 1 — Enhanced Dashboard KPIs (Priority: P1) MVP

**Goal**: Dashboard shows streak, weekly comparison, and top 3 goal progress bars.

**Independent Test**: Log in with sessions across multiple days and 3+ active goals. Dashboard shows all KPI cards with correct data.

### Implementation for User Story 1

- [x] T005 [US1] Rewrite `client/Pages/Dashboard.razor` authenticated section — replace the minimal stats-cards div with a KPI grid: (1) Total Sessions card, (2) Total Hours card, (3) Current Streak card showing `stats.CurrentStreak` days + "Longest: X" subtitle, (4) This Week card showing `stats.ThisWeekHours` hrs with trend arrow comparing to `stats.LastWeekHours`
- [x] T006 [US1] Add goal progress section to `client/Pages/Dashboard.razor` — below KPI cards, show "Active Goals" heading with progress bars for top 3 active goals (fetch via `GoalService.GetActiveGoalsAsync()`, sort by most recent session, take 3). Each shows goal name, currentHours/targetHours, and percentage bar. Include "View all goals" link.
- [x] T007 [US1] Update Dashboard `OnInitializedAsync` in `client/Pages/Dashboard.razor` — replace manual session fetching with `SessionService.GetStatsAsync()` call, also fetch active goals for the progress section
- [x] T008 [US1] Add inject directive for `StudyGoalService` in `client/Pages/Dashboard.razor`
- [x] T009 [US1] Add CSS for dashboard KPI grid and trend arrow in `client/wwwroot/css/app.css` — `.kpi-grid` layout (2x2 on desktop, stacked on mobile), `.trend-up` (green arrow), `.trend-down` (red arrow), `.trend-equal` (gray dash), streak card styling

**Checkpoint**: Dashboard shows all 4 KPI cards + goal progress bars. Empty states work for users with no data.

---

## Phase 4: User Story 2 — Goal Completion Badge (Priority: P2)

**Goal**: Completed goals show a green "Completed" badge and green progress bar.

**Independent Test**: Create a goal with a low target, log enough hours to exceed it, verify green badge and green progress bar on the Goals page.

### Implementation for User Story 2

- [x] T010 [US2] Add completion badge to goal cards in `client/Pages/Goals.razor` — inside the goal name `<h5>`, after the existing Inactive badge check, add: if `goal.CurrentHours >= goal.TargetHours && goal.TargetHours > 0`, render `<span class="badge bg-success ms-2">Completed</span>`
- [x] T011 [US2] Add conditional green progress bar in `client/Pages/Goals.razor` — change the progress bar CSS class: if completed, use `progress-bar bg-success` instead of `progress-bar`. Apply to both desktop and mobile button sections.
- [x] T012 [US2] Add CSS for completed goal styling in `client/wwwroot/css/app.css` — `.goal-completed` subtle green left border or background tint on the list-group-item

**Checkpoint**: Goals at or above target show green badge + green bar. Goals below target look unchanged. Editing target above currentHours removes the badge.

---

## Phase 5: User Story 3 — Toast Notification on Goal Completion (Priority: P3)

**Goal**: Congratulatory toast appears when a session causes a goal to reach 100%.

**Independent Test**: Create a goal near completion, log the finishing session, verify toast appears and auto-dismisses.

### Implementation for User Story 3

- [x] T013 [P] [US3] Create `client/Services/ToastService.cs` — injectable singleton service with: `OnShow` event, `Show(string message, ToastType type)` method, `ToastType` enum (Success, Error, Info)
- [x] T014 [P] [US3] Create `client/Components/Toast.razor` — component that injects `ToastService`, listens to `OnShow`, renders a fixed-position toast at top-right with message and type-based styling (green/red/blue), auto-dismisses after 5 seconds via `Task.Delay` + `StateHasChanged`
- [x] T015 [US3] Register `ToastService` as singleton in `client/Program.cs`
- [x] T016 [US3] Add `<Toast />` component to `client/Layout/MainLayout.razor` — place it outside the `<main>` element so it overlays all pages
- [x] T017 [US3] Add goal completion detection to `client/Pages/StudyLog.razor` — before saving a session, snapshot goal hours for the session's category. After save, re-fetch goals, compare before/after, and call `ToastService.Show()` for any goal that crossed the completion threshold
- [x] T018 [US3] Add CSS for toast component in `client/wwwroot/css/app.css` — `.toast-container` fixed top-right, `.toast-item` with slide-in animation, `.toast-success`/`.toast-error`/`.toast-info` color variants, auto-dismiss fade-out

**Checkpoint**: Logging a finishing session triggers a toast. Already-completed goals don't re-trigger. Toast auto-dismisses and doesn't block interaction.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories.

- [x] T019 [P] Ensure dashboard KPI cards are responsive on mobile — stack to single column, adequate touch padding in `client/wwwroot/css/app.css`
- [x] T020 [P] Add dashboard goal progress empty state — "No active goals" message with link to Goals page in `client/Pages/Dashboard.razor`
- [x] T021 Run quickstart.md validation — verify all scenarios from quickstart.md pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on T001 (API model) — BLOCKS US1 dashboard
- **User Story 1 (Phase 3)**: Depends on Phase 1 + Phase 2 (needs stats API with new fields)
- **User Story 2 (Phase 4)**: Depends on Phase 1 only (no API change needed — client-side logic)
- **User Story 3 (Phase 5)**: Independent of Phase 2 (no stats dependency) — depends only on Phase 1
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Requires Phase 2 (API streak/weekly data)
- **User Story 2 (P2)**: Can start after Phase 1 — independent of API changes
- **User Story 3 (P3)**: Can start after Phase 1 — independent of API changes

### Within Each User Story

- Models before services
- Services/logic before UI
- UI before styling

### Parallel Opportunities

- Setup: T001 and T002 can run in parallel (different projects)
- US3: T013 and T014 can run in parallel (service vs component)
- US2 and US3 can start in parallel after Phase 1 (independent features)
- Polish: T019 and T020 can run in parallel

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (model extensions)
2. Complete Phase 2: Foundational (API streak + weekly calculation)
3. Complete Phase 3: User Story 1 (dashboard KPIs)
4. **STOP and VALIDATE**: Test dashboard with real data
5. Deploy — dashboard is immediately more useful

### Incremental Delivery

1. Setup + Foundational → API ready with streak/weekly data
2. Add User Story 1 → Test → Deploy (enhanced dashboard)
3. Add User Story 2 → Test → Deploy (goal completion badge)
4. Add User Story 3 → Test → Deploy (toast notifications)
5. Polish → Test → Deploy (responsive, empty states)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- US2 (goal badge) is purely client-side — no API change needed
- US3 (toast) requires a new component + service + integration in StudyLog
- Commit after each task or logical group
- Stop at any checkpoint to validate independently
