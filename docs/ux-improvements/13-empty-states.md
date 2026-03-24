# 13 — Empty States

## Priority: Medium

## Problem

When a list has no items (no trainings, no members, no locations), the page shows nothing — just a blank area. This is confusing for new users and doesn't guide them toward the next action. Empty states are an opportunity to onboard and direct users.

## Affected Roles

- **All roles** — Anyone encountering an empty list

## Use Cases

### UC-1: Trainer sees empty training list

As a trainer, when there are no trainings yet, I want to see guidance instead of a blank page.

- Instead of an empty grid, I see a friendly message: "No trainings yet"
- Below it, a "Create Training" button invites me to get started
- Optional: a brief explanation of what trainings are for

### UC-2: Member sees no upcoming trainings

As a member, when there are no upcoming trainings I've joined, I want to know what to do.

- My schedule shows: "You haven't joined any trainings yet"
- A "Browse Trainings" link guides me to the training list

### UC-3: Admin sees empty member list

As an admin, when filtering shows no results, I want clarity.

- The member list shows: "No members match your filters"
- A "Clear filters" button helps me reset
- Distinct from "No members registered yet" (which would show for a truly empty list)

### UC-4: Trainer sees no recurring trainings

As a trainer, the recurring training page shows: "No recurring trainings yet — create one to schedule regular sessions."

### UC-5: Empty search results

As any user, when a search returns nothing, I see "No results for [search term]" with suggestions to broaden the search.

## Acceptance Criteria

- Every list page has a meaningful empty state
- Empty states include a relevant message and a call-to-action where appropriate
- "No results" (from filtering) is distinct from "No items exist" (empty collection)
- Empty states match the visual style of the rest of the app
- CTAs in empty states are role-appropriate (members don't see "Create Training")
