# Feature Specification: Goal Completion & Dashboard KPIs

**Feature Branch**: `002-goal-completion-dashboard-kpis`
**Created**: 2026-04-03
**Status**: Draft
**Input**: User description: "Goal completion celebrations (visual badge + toast notification on 100% progress, keep goal visible with Completed badge) and enhanced dashboard KPIs (study streak with current/longest, weekly hours comparison with trend arrow, top 3 active goal progress bars)"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Enhanced Dashboard KPIs (Priority: P1)

An authenticated user opens the Dashboard and immediately sees a rich
overview of their study habits: their current study streak (consecutive
days), their longest streak ever, how this week compares to last week
(with a visual trend indicator), and progress bars for their top 3
active goals. This replaces the current minimal view of just total
sessions and total hours, giving users actionable motivation at a
glance.

**Why this priority**: The dashboard is the first page users see after
login. Enriching it with meaningful KPIs has the highest visibility and
motivational impact of any change in this feature set.

**Independent Test**: Log in with at least 3 active goals and sessions
across multiple days. Verify the dashboard shows: current streak,
longest streak, this week vs last week hours with a trend arrow, and
progress bars for the top 3 goals.

**Acceptance Scenarios**:

1. **Given** a user has studied for 5 consecutive days, **When** they
   open the Dashboard, **Then** they see "Current Streak: 5 days" and
   their longest streak value.
2. **Given** a user studied 8 hours this week and 5 hours last week,
   **When** they view the Dashboard, **Then** they see "8.0 hrs" with
   an upward trend arrow and "+3.0 hrs" or similar comparison.
3. **Given** a user has 4 active goals, **When** they view the
   Dashboard, **Then** they see progress bars for the 3 goals with the
   most recent activity, each showing goal name, current/target hours,
   and a percentage bar.
4. **Given** a user has no sessions at all, **When** they view the
   Dashboard, **Then** streak shows "0 days", weekly summary shows
   "0 hrs", and goals section shows "No active goals" or links to
   the Goals page.

---

### User Story 2 - Goal Completion Badge (Priority: P2)

A user who has reached or exceeded their target hours on a goal sees a
clear visual indicator that the goal is complete. The goal card displays
a "Completed" badge with a distinct color (green), and the progress bar
shows 100% with a completed style. The goal remains visible and active
until the user manually marks it inactive — completion is informational,
not a state change.

**Why this priority**: Visual feedback for completed goals is a core
motivational feature, but it only affects users who have reached a
target — fewer users see this immediately compared to dashboard KPIs.

**Independent Test**: Create a goal with a 5-hour target, log 5+ hours
against it. Navigate to the Goals page and verify the goal shows a
green "Completed" badge and the progress bar is visually distinct.

**Acceptance Scenarios**:

1. **Given** a goal has currentHours >= targetHours, **When** the user
   views the Goals page, **Then** the goal card displays a green
   "Completed" badge next to the goal name.
2. **Given** a goal is completed, **When** the user views it, **Then**
   the progress bar shows 100% with a success/green color instead of
   the default blue.
3. **Given** a goal is completed, **When** the user views it, **Then**
   the goal remains active and editable — the user can still log more
   hours or change the target.
4. **Given** a goal was completed but the user increases the target
   hours above current hours, **When** they view the goal, **Then**
   the "Completed" badge disappears and the progress bar returns to
   normal.

---

### User Story 3 - Toast Notification on Goal Completion (Priority: P3)

When a user logs a study session that pushes a goal's cumulative hours
to or past the target, a congratulatory toast notification appears
briefly (auto-dismissing after 5 seconds). The toast celebrates the
achievement with the goal name and a success message. This is the
foundation for a reusable toast system that can later be used for CRUD
feedback across the app.

**Why this priority**: The toast is a delightful moment but only occurs
once per goal completion. It also requires building a new reusable
component (toast service), making it the most effort for a single-use
interaction. However, the reusable toast infrastructure adds future
value.

**Independent Test**: Create a goal with a 2-hour target. Log a session
of 2+ hours against it. Verify a congratulatory toast appears at the
top-right of the screen and auto-dismisses after 5 seconds.

**Acceptance Scenarios**:

1. **Given** a goal is at 1.5/2.0 hours, **When** the user logs a 1-hour
   session for that goal, **Then** a toast appears saying something like
   "Goal completed: [Goal Name]!" with a success style.
2. **Given** a goal is already completed (currentHours >= targetHours),
   **When** the user logs another session, **Then** no additional
   completion toast appears (it only fires on the transition to 100%).
3. **Given** the user is on any page, **When** a toast is triggered,
   **Then** it appears in a consistent position (top-right), is
   non-blocking, and auto-dismisses after 5 seconds.
4. **Given** the user has multiple goals that complete simultaneously
   (unlikely but possible with a large session), **When** they log the
   session, **Then** each completed goal gets its own toast, stacked.

---

### Edge Cases

- **User with no goals**: Dashboard goal progress section shows a
  prompt to create a goal, linking to the Goals page.
- **User with no sessions**: Streak shows 0, weekly shows 0, goal
  progress bars are at 0%.
- **Goal target is 0**: Should not be possible (validation: 1-10,000),
  but if encountered, treat as not completable.
- **Multiple sessions on the same day**: Count as one day for streak
  purposes, hours are summed.
- **Sessions logged for past dates (backfill)**: Streak calculation
  must use session dates, not creation dates, so backfilling can
  retroactively extend a streak.
- **Week boundary**: "This week" is Monday through Sunday. "Last week"
  is the preceding Monday-Sunday.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Dashboard MUST display the user's current study streak
  (consecutive days with at least one session).
- **FR-002**: Dashboard MUST display the user's longest streak ever.
- **FR-003**: Dashboard MUST display hours studied this week compared to
  last week, with a visual trend indicator (up arrow, down arrow, or
  equal sign).
- **FR-004**: Dashboard MUST display progress bars for the top 3 active
  goals, showing goal name, current hours, target hours, and percentage.
- **FR-005**: Dashboard MUST retain the existing total sessions and total
  hours KPIs.
- **FR-006**: Goals page MUST display a green "Completed" badge on any
  goal where currentHours >= targetHours.
- **FR-007**: Completed goals MUST show their progress bar in a success
  color (green) instead of the default color.
- **FR-008**: Completed goals MUST remain active and editable — completion
  is informational, not a status change.
- **FR-009**: A congratulatory toast notification MUST appear when a
  session is logged that causes a goal to reach or exceed its target.
- **FR-010**: The completion toast MUST auto-dismiss after 5 seconds and
  MUST NOT block user interaction.
- **FR-011**: The completion toast MUST only fire on the transition to
  completion (not on subsequent sessions for an already-completed goal).
- **FR-012**: All dashboard data MUST be scoped to the authenticated
  user only.

### Key Entities

- **StudySessionStats (extended)**: Adds currentStreak (int, consecutive
  days), longestStreak (int, all-time), thisWeekHours (double),
  lastWeekHours (double) — computed server-side from session dates.
- **StudyGoal (unchanged)**: Uses existing currentHours and targetHours
  fields to derive completion status client-side. No schema change.
- **Toast**: Ephemeral UI entity — message text, type (success/error/
  info), auto-dismiss duration. Not persisted.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: User can assess their study consistency (streak) within 2
  seconds of viewing the Dashboard.
- **SC-002**: User can determine if they are studying more or less than
  last week within 2 seconds of viewing the Dashboard.
- **SC-003**: User can see progress toward their top goals without
  navigating away from the Dashboard.
- **SC-004**: User knows a goal is complete within 1 second of viewing
  the Goals page (visual badge is immediately visible).
- **SC-005**: User receives instant positive feedback when a session
  completes a goal (toast appears within 1 second of session save).
- **SC-006**: Dashboard loads all KPIs in under 2 seconds.

## Assumptions

- The stats API endpoint will be extended to include streak and weekly
  comparison data, computed server-side for efficiency.
- "Consecutive days" for streak means calendar days with at least one
  session, based on session date (not creation timestamp).
- Week boundaries follow ISO standard: Monday through Sunday.
- The toast component is built as a reusable foundation but only the
  goal-completion trigger is implemented in this feature. Other CRUD
  toasts are deferred to a future iteration.
- Dashboard KPIs use the same `/api/sessions/stats` endpoint, avoiding
  new API endpoints.
- The top 3 goals on the dashboard are selected by most recent session
  activity, not alphabetically or by creation date.
