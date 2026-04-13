# Data Model: Goal Completion & Dashboard KPIs

**Date**: 2026-04-03
**Feature**: 002-goal-completion-dashboard-kpis

## Existing Entities (no schema changes)

### StudySession

Unchanged. Session dates are the source for streak calculation.

| Field    | Type     | Notes                        |
| -------- | -------- | ---------------------------- |
| Id       | string   | GUID                         |
| UserId   | string   | Partition key                |
| Category | string   | Links to goal by name        |
| Hours    | double   | Must be > 0                  |
| Date     | DateTime | Session date                 |

### StudyGoal

Unchanged. Completion is derived from `CurrentHours >= TargetHours`
(client-side only — no new field).

| Field        | Type      | Notes                        |
| ------------ | --------- | ---------------------------- |
| Id           | string    | GUID                         |
| UserId       | string    | Partition key                |
| Name         | string    | Goal/subject name            |
| TargetHours  | double    | 1-10,000                     |
| CurrentHours | double    | Calculated from sessions     |
| IsActive     | bool      | Active/inactive toggle       |

## Extended Entities (API response — not persisted)

### StudySessionStats (extended)

Four new fields added to the existing stats response.

| Field            | Type                       | Status   | Notes |
| ---------------- | -------------------------- | -------- | ----- |
| TotalSessions    | int                        | Existing | |
| TotalHours       | double                     | Existing | |
| AverageHoursPerDay | double                   | Existing | |
| HoursByCategory  | Dictionary<string, double> | Existing | |
| DailyBreakdown   | List\<DailyHours\>         | Existing | |
| CurrentStreak    | int                        | **New**  | Consecutive days studied up to today |
| LongestStreak    | int                        | **New**  | All-time longest consecutive days |
| ThisWeekHours    | double                     | **New**  | Hours Mon-Sun of current week |
| LastWeekHours    | double                     | **New**  | Hours Mon-Sun of previous week |

## New Entities (client-side only — not persisted)

### ToastMessage

Ephemeral UI entity for the toast notification system.

| Field    | Type   | Notes                              |
| -------- | ------ | ---------------------------------- |
| Id       | string | GUID for keying in the render loop |
| Message  | string | Display text                       |
| Type     | enum   | Success, Error, Info               |
| Duration | int    | Auto-dismiss milliseconds (5000)   |

## Derived States (client-side logic)

### Goal Completion

- **Condition**: `goal.CurrentHours >= goal.TargetHours`
- **Visual**: Green "Completed" badge + green progress bar
- **Not a persisted field** — computed on render

### Weekly Trend

- **Condition**: Compare `ThisWeekHours` vs `LastWeekHours`
- **Visual**: Up arrow if this > last, down arrow if this < last,
  equal if same
- **Difference**: `ThisWeekHours - LastWeekHours` shown as "+X.X" or
  "-X.X"

## Validation Rules

- Streak counts calendar days, not sessions. Multiple sessions on one
  day count as one day.
- A day with at least one session with hours > 0 counts toward streak.
- Current streak counts backward from today. If today has no session,
  count from yesterday.
- Week boundary: Monday 00:00 UTC to Sunday 23:59 UTC.
- Streak and weekly data are computed from ALL sessions (not limited
  to 30-day window).
