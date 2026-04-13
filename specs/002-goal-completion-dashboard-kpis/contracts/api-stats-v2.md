# API Contract: Session Stats Endpoint v2

**Date**: 2026-04-03
**Feature**: 002-goal-completion-dashboard-kpis

## GET /api/sessions/stats (extended)

Same endpoint as before, with 4 new fields in the response.
Fully backward-compatible — existing consumers see new fields but
are not broken by them.

### Response: 200 OK

```json
{
  "totalSessions": 42,
  "totalHours": 128.5,
  "averageHoursPerDay": 4.28,
  "hoursByCategory": {
    "Mathematics": 45.0,
    "Computer Science": 60.5
  },
  "dailyBreakdown": [
    { "date": "2026-03-04T00:00:00", "hours": 2.5 },
    { "date": "2026-03-05T00:00:00", "hours": 0.0 }
  ],
  "currentStreak": 5,
  "longestStreak": 12,
  "thisWeekHours": 8.0,
  "lastWeekHours": 5.5
}
```

### New Fields

| Field          | Type   | Description |
| -------------- | ------ | ----------- |
| currentStreak  | int    | Consecutive calendar days with at least one session, counting backward from today (or yesterday if today has no session) |
| longestStreak  | int    | All-time longest run of consecutive study days |
| thisWeekHours  | double | Total hours for current week (Monday-Sunday) |
| lastWeekHours  | double | Total hours for previous week (Monday-Sunday) |

### Behavior Notes

- Streak and weekly fields are computed from ALL user sessions, not
  just the date range specified by `from`/`to` query parameters.
- If the user has no sessions, all four fields return 0.
- Multiple sessions on the same day count as one streak day.
- Week boundaries: Monday 00:00 UTC to Sunday 23:59 UTC (ISO week).
