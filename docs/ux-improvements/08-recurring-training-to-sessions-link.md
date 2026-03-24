# 08 — Recurring Training to Sessions Link

## Priority: High

## Problem

After a trainer generates sessions from a recurring training template, there is no way to see those sessions. The recurring training card shows "Last generated until" but provides no link to the actual sessions. The trainer has to leave the page and manually find sessions elsewhere (which currently doesn't exist either — see #01).

## Affected Roles

- **Trainer** — Needs to verify and manage generated sessions
- **Admin** — Needs oversight of all sessions in a series

## Use Cases

### UC-1: Trainer views generated sessions after generating

As a trainer, after I generate sessions for a recurring training, I want to immediately see what was created.

- I click "Generate Sessions" and pick a date
- After generation completes, I'm either shown the sessions directly or offered a link to view them
- I can verify the dates, times, and count are correct

### UC-2: Trainer navigates from recurring training to its sessions

As a trainer, I want to quickly see all sessions belonging to a recurring training template.

- On the recurring training card or detail, there's a "View Sessions" button
- Clicking it shows me a list of sessions filtered to this recurring training
- I see each session's date, status, participant count

### UC-3: Trainer spots gaps or issues in the schedule

As a trainer, I want to scan generated sessions to spot holidays or conflicts.

- The session list for a recurring training shows all dates in order
- I can quickly identify a session that falls on a holiday
- I can cancel individual sessions without affecting the rest

### UC-4: Trainer sees session generation status

As a trainer, I want to know how far ahead sessions have been generated.

- The recurring training card shows "Sessions generated until: [date]"
- If that date is approaching, I see a visual cue to generate more
- I can quickly generate another batch from the same card

## Acceptance Criteria

- Recurring training cards have a "View Sessions" action
- Clicking it navigates to a session list pre-filtered by recurring training ID
- After generating sessions, the user is directed to or shown the resulting sessions
- Session count is visible on the recurring training card
- Works hand-in-hand with #01 (Session List & Detail Page)
