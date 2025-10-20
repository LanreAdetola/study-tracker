
# Study Tracker

## Phase 2 — Azure Setup & Authentication
✅ **Completed**

### Goals for Phase 2
- Enable user authentication  
- Deploy the frontend to Azure Static Web Apps  
- Prepare groundwork for secure per-user data  

### Work Completed
- Created Azure Static Web App and connected to GitHub repository  
- Enabled built-in authentication (GitHub / Microsoft login)  
- Added login/logout functionality in Blazor frontend  
- Displayed “Welcome, [username]” after login  
- Verified that Phase 1 dummy data renders correctly for authenticated users  

### Updated Monorepo Structure (if any)
```
study-tracker/
├── client/                     # Blazor WASM frontend (Phase 2 features added)
├── api/                        # Azure Functions backend (still empty)
├── .github/workflows/          # CI/CD workflows (future)
└── README.md                   # Documentation for Phase 2
```

### Notes for Future Phases
- Phase 3 → Connect to Cosmos DB and build Azure Functions API  
- Phase 4 → Charts, filters, and export functionality  
- Phase 5 → Smart insights with AI (optional)  
- Phase 6 → CI/CD, polish, and public portfolio deployment  

> This README is specific to Phase 2 and will be updated as each phase is completed, allowing others to follow branch-by-branch.
