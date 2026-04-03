<!--
  Sync Impact Report
  ==================
  Version change: 0.0.0 → 1.0.0 (initial ratification)

  Added principles:
    - I. Simplicity First
    - II. Data Integrity
    - III. User Isolation
    - IV. Incremental Development
    - V. Observability

  Added sections:
    - Constraints
    - Definition of Done

  Removed sections: none (initial version)

  Templates requiring updates:
    - .specify/templates/plan-template.md — ✅ no updates needed (generic template)
    - .specify/templates/spec-template.md — ✅ no updates needed (generic template)
    - .specify/templates/tasks-template.md — ✅ no updates needed (generic template)

  Follow-up TODOs: none
-->

# Study Tracker Constitution

## Purpose

This project exists to help users consistently track and improve their study
habits through structured logging, goal setting, and data-driven insights.

## Core Principles

### I. Simplicity First

Features MUST be understandable and usable without explanation. Avoid
unnecessary complexity in UI and backend logic. Choose the simplest
implementation that meets the current requirement.

### II. Data Integrity

All data MUST be validated both client-side and server-side. Invalid or
inconsistent data MUST never be persisted. The API MUST NOT trust client
input — server-side validation is the enforcement boundary.

### III. User Isolation

Each user's data MUST remain strictly isolated. No cross-user data access
is permitted. All API endpoints MUST scope operations to the authenticated
user's identity via the `x-ms-client-principal-id` header.

### IV. Incremental Development

Features are built in small, testable increments. Each feature MUST provide
standalone value. Breaking changes to existing API contracts MUST be avoided;
new functionality MUST use additive changes wherever possible.

### V. Observability

System behavior MUST be traceable through logging and monitoring. Application
Insights is the primary observability tool. Errors and key operations MUST
produce structured log entries.

## Constraints

- Maximum 50 users (free-tier constraint)
- Cosmos DB as primary datastore
- Azure Functions as backend
- Authentication via GitHub OAuth

## Definition of Done

A feature is complete when:

- It meets its specification
- It passes validation rules
- It is deployed and functional in production
- It does not break existing functionality

## Non-Goals

- Real-time collaboration
- Complex social features
- Mobile-native apps (for now)

## Governance

This constitution defines the non-negotiable principles for the Study Tracker
project. All implementation decisions — whether made by a human developer or
an AI assistant — MUST comply with these principles.

- **Amendments:** Any change to this constitution MUST be documented with a
  version bump, a rationale, and a review of dependent artifacts for
  consistency.
- **Versioning:** This document follows semantic versioning. MAJOR for
  principle removals or redefinitions, MINOR for new principles or material
  expansions, PATCH for clarifications and wording fixes.
- **Compliance:** Every spec, plan, and task list produced by the SpecKit
  workflow MUST include a Constitution Check verifying alignment with these
  principles before implementation begins.

**Version**: 1.0.0 | **Ratified**: 2026-04-03 | **Last Amended**: 2026-04-03
