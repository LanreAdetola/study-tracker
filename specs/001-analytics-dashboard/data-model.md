# Data Model: Study Analytics Dashboard

**Date**: 2026-04-03
**Feature**: 001-analytics-dashboard

## Existing Entities (no changes)

### StudySession

Source of truth for all analytics data. Stored in Cosmos DB `sessions`
container with partition key `/userId`.

| Field     | Type     | Notes                        |
| --------- | -------- | ---------------------------- |
| Id        | string   | GUID                         |
| UserId    | string   | Partition key                |
| Category  | string   | Subject name or custom label |
| Hours     | double   | Must be > 0                  |
| Notes     | string   | Optional                     |
| Date      | DateTime | Must not be in the future    |
| CreatedAt | DateTime | UTC                          |
| UpdatedAt | DateTime | UTC                          |

### StudyGoal

Used for goal progress visualization. Stored in Cosmos DB `goals` container
with partition key `/userId`.

| Field        | Type      | Notes                     |
| ------------ | --------- | ------------------------- |
| Id           | string    | GUID                      |
| UserId       | string    | Partition key             |
| Name         | string    | Goal/subject name         |
| Type         | string    | "Subject" or "Certification" |
| TargetHours  | double    | 1-10,000                  |
| CurrentHours | double    | Calculated from sessions  |
| TargetDate   | DateTime? | Optional deadline         |
| IsActive     | bool      | Active/inactive toggle    |
| CreatedAt    | DateTime  | UTC                       |
| UpdatedAt    | DateTime  | UTC                       |

## New Entities (API response models — not persisted)

### StudySessionStats

Returned by `GET /api/sessions/stats`. Aggregated server-side from the
sessions container.

| Field            | Type                    | Notes                             |
| ---------------- | ----------------------- | --------------------------------- |
| TotalSessions    | int                     | Count of sessions in date range   |
| TotalHours       | double                  | Sum of hours in date range        |
| AverageHoursPerDay | double                | TotalHours / days in range        |
| HoursByCategory  | Dictionary<string, double> | Category name → total hours    |
| DailyBreakdown   | List\<DailyHours\>     | One entry per day in range        |

### DailyHours

Nested within StudySessionStats.DailyBreakdown.

| Field | Type     | Notes                      |
| ----- | -------- | -------------------------- |
| Date  | DateTime | The calendar date          |
| Hours | double   | Total hours studied that day |

### GoalProgressPoint (client-side only)

Used to build the goal progress line chart. Computed client-side from
sessions and goals data.

| Field           | Type     | Notes                          |
| --------------- | -------- | ------------------------------ |
| Date            | DateTime | Session date                   |
| CumulativeHours | double   | Running total toward goal      |
| GoalName        | string   | Which goal this point belongs to |
| TargetHours     | double   | The goal's target for reference |

## Relationships

```
StudySession.Category ←→ StudyGoal.Name
  (Sessions link to goals by matching the category name to the goal name)

StudySessionStats ← derived from → StudySession (server-side aggregation)
GoalProgressPoint ← derived from → StudySession + StudyGoal (client-side)
```

## Validation Rules

- Stats endpoint: `from` date defaults to 30 days ago if not provided.
- Stats endpoint: `to` date defaults to today if not provided.
- Sessions with future dates are excluded from aggregation.
- Sessions with hours <= 0 are excluded from aggregation.
- All queries scoped to authenticated userId (partition key).
