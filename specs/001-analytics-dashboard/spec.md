# Feature Specification: Study Analytics Dashboard

**Feature Branch**: `001-analytics-dashboard`
**Created**: 2026-04-03
**Status**: Implemented
**Input**: User description: "Provide users with insights into their study habits over time through daily study hours, category distribution, and goal progress visualizations."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Daily Study Hours (Priority: P1)

A user opens the Analytics page to see how many hours they studied each day
over the last 30 days. They see a time-series chart that reveals their study
patterns — which days were productive and which had gaps. This helps them
identify consistency trends and adjust their schedule.

**Why this priority**: This is the core insight — understanding daily study
volume over time is the primary reason users visit an analytics page.

**Independent Test**: Navigate to the Analytics page with at least 5 study
sessions logged across different dates. A time-series visualization appears
showing hours per day for the last 30 days. Days with no sessions show as
zero.

**Acceptance Scenarios**:

1. **Given** a user has 10 sessions across 7 different days in the last 30
   days, **When** they open the Analytics page, **Then** they see a
   time-series view with hours plotted per day for the last 30 days.
2. **Given** a user has sessions only from 15+ days ago, **When** they view
   the chart, **Then** recent days show zero hours and older days show
   recorded values.
3. **Given** a user has multiple sessions on the same day, **When** the chart
   renders, **Then** that day's value is the sum of all session hours.

---

### User Story 2 - View Category Breakdown (Priority: P2)

A user wants to understand how their study time is distributed across
subjects. They see a breakdown showing the proportion of hours spent on each
category (linked goals or custom categories). This helps them ensure they
are balancing their study across subjects.

**Why this priority**: Category distribution is the second most requested
insight after volume trends — it answers "where is my time going?"

**Independent Test**: Navigate to the Analytics page with sessions across at
least 3 different categories. A category breakdown visualization appears
showing total hours per category with proportional sizing.

**Acceptance Scenarios**:

1. **Given** a user has sessions in 4 categories, **When** they view the
   category breakdown, **Then** each category is displayed with its total
   hours and relative proportion.
2. **Given** a user has all sessions in one category, **When** they view
   the breakdown, **Then** that single category shows as 100% of study time.
3. **Given** a user has sessions linked to goals and sessions with custom
   categories, **When** they view the breakdown, **Then** both goal-linked
   and custom categories appear in the same visualization.

---

### User Story 3 - View Goal Progress Over Time (Priority: P3)

A user with active study goals wants to see how their progress has
accumulated over time — not just the current percentage, but the trajectory.
They see a visualization showing cumulative hours toward each active goal,
helping them understand if they are on pace to hit their targets.

**Why this priority**: Goal progress adds motivational value and ties the
analytics page to the existing goals feature, but requires both sessions and
goals data, making it more complex.

**Independent Test**: Navigate to the Analytics page with at least 2 active
goals and sessions linked to each. A progress visualization appears showing
cumulative hours over time for each goal alongside the target.

**Acceptance Scenarios**:

1. **Given** a user has 2 active goals with sessions logged against each,
   **When** they view goal progress, **Then** they see cumulative hours
   plotted over time for each goal with the target indicated.
2. **Given** a user has a goal with no sessions logged, **When** they view
   goal progress, **Then** that goal appears at zero progress.
3. **Given** a user has only inactive goals, **When** they view the
   Analytics page, **Then** the goal progress section is hidden or shows a
   message indicating no active goals.

---

### Edge Cases

- **No sessions at all**: Analytics page shows a friendly empty state with
  guidance to log their first session (link to Study Log page).
- **Large number of sessions**: The 30-day window naturally bounds the daily
  chart. Category breakdown aggregates all sessions regardless of date range.
- **Sessions with future dates**: Cannot occur — existing validation prevents
  future dates. If invalid data exists, it is excluded from calculations.
- **Single day of data**: Time-series still renders the full 30-day window
  with one data point and 29 zero-value days.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display daily study hours for the last 30 days on
  the Analytics page.
- **FR-002**: System MUST aggregate multiple sessions on the same day into a
  single daily total.
- **FR-003**: System MUST display a category breakdown showing total hours
  per study category.
- **FR-004**: Category breakdown MUST include both goal-linked sessions and
  custom-category sessions.
- **FR-005**: System MUST display cumulative goal progress over time for each
  active goal.
- **FR-006**: Goal progress MUST show the target hours as a reference point.
- **FR-007**: System MUST show summary statistics above the charts: total
  hours (last 30 days), total sessions (last 30 days), and average hours per
  day.
- **FR-008**: Analytics page MUST show a designed empty state when the user
  has no sessions, with a call-to-action linking to the Study Log page.
- **FR-009**: All data displayed MUST be scoped to the authenticated user
  only (no cross-user data).
- **FR-010**: Only sessions with valid dates (not in the future) and positive
  hours MUST be included in analytics calculations.

### Key Entities

- **StudySession**: Date, duration (hours), category/goal link, notes —
  the primary data source for all analytics.
- **StudyGoal**: Subject, target hours, current hours, active/inactive
  status — used for goal progress visualization.
- **DailyAggregate**: Derived entity representing total hours studied on a
  given date — computed from sessions, not persisted.
- **CategorySummary**: Derived entity representing total hours per category —
  computed from sessions, not persisted.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: User can identify their most and least productive days within
  5 seconds of viewing the Analytics page.
- **SC-002**: Analytics page fully loads and renders all visualizations in
  under 2 seconds.
- **SC-003**: User can determine which category they spend the most time on
  without reading raw numbers (visual hierarchy communicates proportion).
- **SC-004**: User can assess whether they are on track for a goal by viewing
  the progress visualization, without navigating to the Goals page.
- **SC-005**: A user with no data sees a clear, non-error empty state and
  knows exactly what to do next.

## Assumptions

- Users have at least a few study sessions logged before the Analytics page
  provides meaningful value. The empty state handles the zero-data case.
- The existing session and goals data endpoints provide sufficient data for
  client-side aggregation at the current scale (max 50 users, moderate
  session counts).
- The 30-day window for the daily chart is a sensible default. Extended date
  range selection (week/month/all-time toggle) may be added in a future
  iteration.
- Category names come from either the linked goal's subject name or a
  free-text custom category field on the session. No normalization or merging
  of similar names is performed.
- The Analytics page is accessible only to authenticated users.
  Unauthenticated visitors see the landing page, not analytics.
