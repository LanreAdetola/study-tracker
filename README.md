# Study Tracker

## Project Overview
Study Tracker helps you track study hours for courses, assignments, and certifications. This project is educational and portfolio-focused, demonstrating full-stack Azure development. The tech stack includes Blazor WebAssembly frontend, Azure Functions backend (planned), and Cosmos DB (planned).

## Monorepo Structure
```
study-tracker/
├── client/                     # Blazor WASM frontend (Phase 1 complete)
├── api/                        # Azure Functions backend (empty for now)
├── .github/workflows/          # CI/CD workflows (future)
└── README.md                   # Documentation
```

## Phase 1 — Setup & Core Foundation
✅ **Completed**

- Monorepo structure initialized (`client` + `api`)  
- Blazor WASM project running successfully in `/client`  
- Added dummy data model (`StudySession`)  
- Implemented `DummyDataService` with sample study sessions  
- Displayed study sessions in table format on `Index.razor`  
- All changes committed to `phase-1-setup` branch

## Notes for Future Phases
- Phase 2 → Authentication and Azure Static Web App deployment  
- Phase 3 → Cosmos DB integration and Azure Functions API  
- Phase 4 → Charts, filters, and export functionality  
- Phase 5 → Smart insights with AI (optional)  
- Phase 6 → CI/CD, polish, and public portfolio deployment  

> This README will be updated as each phase is completed, allowing others to follow branch-by-branch.

