# 06 — List Filtering & Search

## Priority: High

## Problem

All list pages (trainings, members, locations, sessions) are unfiltered flat lists that load page 1 with no way to search or narrow results. As the club grows, these lists become unusable. Members can't find specific trainings, trainers can't find specific members, and nobody can search by date, name, or status.

## Affected Roles

- **Member** — Needs to find trainings by date, type, availability
- **Trainer** — Needs to find members, filter trainings by status
- **Admin** — Needs to find pending members, filter by status

## Use Cases

### UC-1: Member searches for trainings by keyword

As a member, I want to search for trainings by name so I can find the specific training I'm looking for (e.g. "Yoga", "Self-Defense").

- On the training list, I type a search term
- The list filters to show only trainings matching my search
- Matching happens on title and description

### UC-2: Member filters trainings by date

As a member, I want to see trainings happening in a specific week or date range so I can plan my schedule.

- I select a date range (this week, next week, custom range)
- Only trainings within that range are shown
- Default view shows upcoming trainings (not past ones)

### UC-3: Member filters trainings by availability

As a member, I want to see only trainings I can still join (not full, not canceled).

- I toggle a filter like "Available to join"
- Full, canceled, and completed trainings are hidden
- I only see trainings with open spots or waitlist option

### UC-4: Trainer filters trainings by status

As a trainer, I want to see only my draft trainings so I can review what needs publishing.

- I filter by status: Draft / Published / Completed / Canceled
- I can also filter "My trainings" to see only those where I'm assigned as trainer

### UC-5: Admin searches for a member

As an admin, I want to find a specific member by name or email.

- On the member list, I type a name or email fragment
- The list filters to matching members
- I can also filter by status (Pending / Approved / Suspended)

### UC-6: Admin filters pending approvals

As an admin, I want to quickly see all members awaiting approval.

- I filter the member list to "Pending" status
- Or I have a dedicated section/badge showing the count of pending members

## Acceptance Criteria

- All list pages have a search/filter bar at the top
- Training list supports: text search, date range, status, availability filter
- Member list supports: text search, status filter, role filter
- Filters update the list in real-time or on apply
- Active filters are visually indicated (chips, highlighted buttons)
- Clearing filters returns to the default view
- Filter state is preserved when navigating back to the list
