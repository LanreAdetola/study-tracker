# Study Tracker

A full-stack study tracking web application built with Blazor WebAssembly and Azure Functions, deployed on Azure Static Web Apps with GitHub OAuth authentication and Cosmos DB persistence.

**Live Demo:** [kind-sky-0c0b6ea03.3.azurestaticapps.net](https://kind-sky-0c0b6ea03.3.azurestaticapps.net/)

---

## Tech Stack

| Layer | Technology |
| --- | --- |
| Frontend | Blazor WebAssembly (.NET 9.0), Bootstrap 5, Chart.js v4 |
| Backend | Azure Functions v4 (.NET 8.0, isolated worker) |
| Database | Azure Cosmos DB (NoSQL) |
| Authentication | GitHub OAuth via Azure Static Web Apps |
| Hosting | Azure Static Web Apps |
| CI/CD | GitHub Actions with OIDC token authentication |
| Monitoring | Application Insights |

---

## Features

### Study Sessions

- Log study sessions with category, hours, date, and optional notes
- Edit and delete sessions with confirmation modals
- Link sessions to active goals or enter a custom category
- Client-side and server-side validation (e.g. no future dates, hours > 0)

### Study Goals

- Create up to 5 goals per user (Subject or Certification types)
- Set target hours (1–10,000) and optional target dates
- Visual progress bars showing current hours vs. target
- Toggle goals between active and inactive

### User Management

- GitHub OAuth login — no passwords to manage
- Automatic user registration on first login
- 50-user capacity limit with live counter on the landing page
- Per-user data isolation enforced at the API level

### Analytics

- Daily study hours bar chart (last 30 days)
- Category breakdown donut chart
- Goal progress line chart with target reference lines
- Summary stats: total sessions, total hours, average hours/day
- Designed empty state with call-to-action for new users
- Powered by Chart.js via JavaScript interop

### Dashboard

- Personalized greeting for authenticated users
- Summary stats: total study sessions and total hours logged
- Hero landing page with feature overview for new visitors

### Mobile Optimization

- Session table switches to card layout on small screens
- Touch-friendly buttons (44px min-height) across all pages
- Full-screen modals on mobile for confirmations
- Responsive chart sizing with reduced label density
- Login buttons stack vertically on narrow viewports

---

## Architecture

```
study-tracker/
├── client/                         # Blazor WebAssembly frontend
│   ├── Pages/                      # Route-level pages
│   │   ├── Dashboard.razor         # Landing page & user stats
│   │   ├── StudyLog.razor          # Session tracking
│   │   ├── Goals.razor             # Goal management
│   │   └── Analytics.razor         # Charts & data visualization
│   ├── Components/                 # Reusable UI components
│   │   ├── SessionForm.razor       # Add/edit session form
│   │   ├── SessionTable.razor      # Session list (table + mobile cards)
│   │   ├── GoalForm.razor          # Add/edit goal form
│   │   └── UserRegistration.razor  # Auto-registration on login
│   ├── Models/                     # Shared data models
│   │   ├── StudySession.cs
│   │   ├── StudyGoal.cs
│   │   ├── UserProfile.cs
│   │   └── AnalyticsModels.cs      # Stats & chart data models
│   ├── Services/                   # HTTP client services
│   │   ├── StudySessionService.cs  # Includes GetStatsAsync()
│   │   ├── StudyGoalService.cs
│   │   └── UserService.cs
│   └── wwwroot/
│       ├── js/charts.js            # Chart.js interop functions
│       └── lib/chart.js/           # Chart.js v4 library
│
├── api/                            # Azure Functions backend
│   ├── Functions/                  # HTTP-triggered endpoints
│   │   ├── StudySessionFunctions.cs # Includes stats endpoint
│   │   ├── StudyGoalFunctions.cs
│   │   └── UserProfileFunctions.cs
│   ├── Services/                   # Business logic layer
│   │   ├── StudySessionService.cs  # Includes GetStatsAsync()
│   │   ├── StudyGoalService.cs
│   │   └── UserProfileService.cs
│   ├── Models/
│   │   └── StudySessionStats.cs    # Stats response model
│   └── Program.cs                  # DI container setup
│
├── specs/                          # SpecKit feature specifications
│   └── 001-analytics-dashboard/    # Analytics feature docs
├── staticwebapp.config.json        # SWA routing & runtime config
└── .github/workflows/              # CI/CD pipeline
```

---

## API Endpoints

### Sessions

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/sessions` | List all sessions for user |
| `GET` | `/api/sessions/stats` | Get aggregated stats (totals, daily breakdown, category hours) |
| `GET` | `/api/sessions/{id}` | Get a single session |
| `POST` | `/api/sessions` | Create a new session |
| `PUT` | `/api/sessions/{id}` | Update an existing session |
| `DELETE` | `/api/sessions/{id}` | Delete a session |

The stats endpoint accepts optional `from` and `to` query parameters (ISO 8601 dates) and defaults to the last 30 days.

### Goals

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/goals` | List all goals for user |
| `GET` | `/api/goals/{id}` | Get a single goal |
| `POST` | `/api/goals` | Create a new goal (max 5) |
| `PUT` | `/api/goals/{id}` | Update an existing goal |
| `DELETE` | `/api/goals/{id}` | Delete a goal |

### Users

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/users/count` | Get user count and capacity status |
| `POST` | `/api/users/register` | Register a new user (50-user limit) |
| `GET` | `/api/users/me` | Get current user profile |

All endpoints authenticate via the `x-ms-client-principal-id` header provided by Azure Static Web Apps.

---

## Running Locally

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Azure Static Web Apps CLI](https://github.com/Azure/static-web-apps-cli)

### Steps

```bash
# Clone the repository
git clone https://github.com/LanreAdetola/study-tracker.git
cd study-tracker

# Start the API (requires a Cosmos DB connection string in local.settings.json)
cd api
func start

# In a separate terminal, start the frontend
cd client
dotnet run

# Or use the SWA CLI to run both together
swa start http://localhost:5000 --api-location api
```

Create an `api/local.settings.json` with your Cosmos DB connection:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "CosmosDBConnectionString": "<your-cosmos-db-connection-string>"
  }
}
```

---

## Deployment

The app deploys automatically via GitHub Actions on every push to `main`.

The workflow:

1. Checks out the repository
2. Authenticates with Azure using OIDC
3. Builds the Blazor frontend and Azure Functions API
4. Deploys both to Azure Static Web Apps

Pull requests get their own staging environments, which are automatically cleaned up when the PR is closed.

---

## Roadmap

- [x] **Phase 1** — Blazor frontend scaffold with dummy data
- [x] **Phase 2** — Azure deployment, GitHub auth, Cosmos DB integration
- [x] **Phase 3** — Full CRUD pages for sessions, goals, and user management
- [x] **Phase 4** — Analytics dashboard with charts (bar, donut, line) and stats endpoint
- [ ] **Phase 4.5** — Filtering, search, and CSV data export
- [ ] **Phase 5** — AI-powered study insights
- [ ] **Phase 6** — CI/CD hardening, polish, and portfolio presentation

---

## Author

Built by [Lanre Adetola](https://github.com/LanreAdetola)
