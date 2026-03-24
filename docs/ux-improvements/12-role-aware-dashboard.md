# 12 — Role-Aware Dashboard

## Priority: Medium

## Problem

The home page is a static welcome message with no actionable content. Every time a user opens the app, they have to navigate somewhere else to do anything. There's no at-a-glance overview of what matters right now — pending approvals for admins, upcoming sessions for members, draft trainings for trainers.

## Affected Roles

- **Member** — Wants to see today's and upcoming trainings
- **Trainer** — Wants to see their upcoming teaching schedule and drafts
- **Admin** — Wants to see pending approvals and system status

## Use Cases

### UC-1: Member sees their upcoming trainings on the home page

As a member, when I open the app, I want to immediately see what's coming up for me.

- The dashboard shows my next 3-5 upcoming trainings/sessions
- Each entry shows: title, date/time, location
- I can click any entry to go to its detail page
- If nothing is coming up, I see a prompt to browse available trainings

### UC-2: Trainer sees their teaching load

As a trainer, I want to see what I'm teaching this week and what needs my attention.

- The dashboard shows my upcoming sessions where I'm the trainer
- I see any draft trainings that still need publishing
- I see any pending participant requests awaiting my approval
- Counts are shown as badges or numbers

### UC-3: Admin sees pending approvals

As an admin, I want to know immediately if members are waiting for approval.

- The dashboard shows a count of pending member approvals
- Clicking it takes me to the filtered member list
- If there are no pending approvals, this section is either hidden or shows "All caught up"

### UC-4: Admin sees system overview

As an admin, I want a quick overview of active trainings, members, and locations.

- Summary cards showing: total active members, upcoming trainings this week, active locations
- These are quick orientation numbers, not detailed reports

### UC-5: Unauthenticated user gets a landing page

As a visitor, I want to understand what this app is and how to get started.

- The landing page explains the training club
- Clear call-to-action to log in or register
- Maybe show upcoming public trainings as a teaser

## Acceptance Criteria

- Home page content adapts based on authentication state and roles
- Members see their upcoming trainings
- Trainers see teaching schedule + items needing attention (drafts, pending participants)
- Admins see pending approvals count + system summary
- Dashboard items link to the relevant detail pages
- Unauthenticated users see a welcoming landing page with login CTA
