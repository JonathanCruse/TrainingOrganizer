# 15 — Pagination Controls

## Priority: Medium

## Problem

The API supports paginated responses (page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage), but the UI always loads page 1 with no way to navigate to subsequent pages. Once the club has more trainings, members, or sessions than fit on one page, users can't access the rest.

## Affected Roles

- **All roles** — Anyone browsing lists

## Use Cases

### UC-1: Member browses past trainings

As a member, I want to page through older trainings when the first page only shows the most recent ones.

- At the bottom of the training list, I see page controls
- I can go to page 2, 3, etc.
- I see the total count so I know how many trainings exist

### UC-2: Admin browses large member list

As an admin, when the club has 200+ members, I need pagination to navigate the list.

- Page controls at the bottom of the member table
- I can choose page size (10, 25, 50)
- I see "Showing 1-25 of 213 members"

### UC-3: Trainer browses session history

As a trainer, I want to browse past sessions for a recurring training spanning several months.

- Sessions are paginated
- I can navigate to earlier sessions

## Acceptance Criteria

- All list pages (trainings, sessions, members, recurring trainings, locations, bookings) have pagination controls
- Pagination shows: current page, total pages, previous/next buttons
- Optional: configurable page size
- Total item count is displayed
- Pagination works together with filters (see #06) — filtering resets to page 1
- Page loads don't reset scroll position unnecessarily
