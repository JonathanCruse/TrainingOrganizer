# 16 — Breadcrumb Navigation

## Priority: Medium

## Problem

Navigation between list and detail pages has no consistent pattern. The training detail has a back button, but there's no visual breadcrumb trail showing where you are in the hierarchy. Users can lose context, especially when navigating between recurring trainings, their sessions, and individual session details.

## Affected Roles

- **All roles** — Anyone navigating the app

## Use Cases

### UC-1: Member navigates from training detail back to list

As a member, after viewing a training detail, I want to easily get back to the training list.

- At the top of the training detail page, I see: Trainings > [Training Title]
- Clicking "Trainings" takes me back to the list
- My previous filter/scroll position is preserved (ideal, not required)

### UC-2: Trainer navigates recurring training hierarchy

As a trainer, I want to understand where I am when viewing a session that belongs to a recurring training.

- Breadcrumb shows: Recurring Trainings > [Template Title] > Sessions > [Session Date]
- I can jump to any level in the hierarchy

### UC-3: Admin navigates location and room structure

As an admin, when viewing room details or bookings for a specific room:

- Breadcrumb shows: Locations > [Location Name] > [Room Name]

## Acceptance Criteria

- Breadcrumbs appear at the top of all detail pages
- Each breadcrumb segment is clickable and navigates to the correct page
- Breadcrumbs reflect the logical hierarchy, not just the URL
- Breadcrumbs use MudBreadcrumbs component for consistent styling
- List pages (top-level) don't need breadcrumbs — just detail/child pages
