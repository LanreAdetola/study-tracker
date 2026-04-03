# API Contract: Session Stats Endpoint

**Date**: 2026-04-03
**Feature**: 001-analytics-dashboard

## GET /api/sessions/stats

Returns aggregated study session statistics for the authenticated user.

### Authentication

Required. Uses `x-ms-client-principal-id` header (Azure Static Web Apps).

### Query Parameters

| Parameter | Type   | Required | Default       | Description              |
| --------- | ------ | -------- | ------------- | ------------------------ |
| from      | string | No       | 30 days ago   | Start date (ISO 8601)    |
| to        | string | No       | Today          | End date (ISO 8601)      |

### Response: 200 OK

```json
{
  "totalSessions": 42,
  "totalHours": 128.5,
  "averageHoursPerDay": 4.28,
  "hoursByCategory": {
    "Mathematics": 45.0,
    "Computer Science": 60.5,
    "Physics": 23.0
  },
  "dailyBreakdown": [
    { "date": "2026-03-04T00:00:00", "hours": 2.5 },
    { "date": "2026-03-05T00:00:00", "hours": 0.0 },
    { "date": "2026-03-06T00:00:00", "hours": 3.0 }
  ]
}
```

### Response: 401 Unauthorized

Returned when `x-ms-client-principal-id` header is missing.

```json
{
  "error": "User not authenticated"
}
```

### Behavior Notes

- `dailyBreakdown` includes every day in the date range, including days with
  zero hours (filled with `0.0`).
- `hoursByCategory` only includes categories with at least one session.
- Sessions with future dates or non-positive hours are excluded.
- All data is scoped to the authenticated user's partition.
- This is a new additive endpoint — no existing endpoints are modified.
