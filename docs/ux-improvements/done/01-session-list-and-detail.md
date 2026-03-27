# 01 — Session List & Detail Page

## Priority: Critical

## Problem

Recurring trainings generate sessions (e.g. every Monday at 18:00), but members have no way to see, browse, or join these sessions in the UI. The entire recurring training flow is broken from the member's perspective — trainers create templates and generate sessions, but the result is invisible.

## Affected Roles

- **Member** — Cannot discover or join weekly/recurring sessions
- **Trainer** — Cannot see which sessions were generated, manage individual sessions
- **Admin** — No oversight of generated sessions

## Use Cases

### UC-1: Member browses upcoming sessions

As a member, I want to see a list of upcoming training sessions so I can decide which ones to attend.

- I navigate to a "Sessions" or "Upcoming Trainings" page
- I see sessions grouped or sorted by date (nearest first)
- Each session shows: title, date/time, location/room, trainer(s), available spots
- I can quickly tell which sessions still have capacity vs. which are full

### UC-2: Member joins a session

As a member, I want to join a specific session so I'm registered for that occurrence.

- From the session list or detail, I click "Join"
- If there's capacity, I'm confirmed immediately
- If full, I'm placed on the waitlist and see my position
- I get feedback (snackbar/toast) confirming my action

### UC-3: Member leaves a session

As a member, I want to leave a session I previously joined, in case my plans change.

- From the session detail, I click "Leave"
- My spot is freed and the next waitlisted person is promoted
- I see confirmation of leaving

### UC-4: Trainer views sessions for a recurring training

As a trainer, I want to see all sessions generated from a recurring training template so I can manage individual occurrences.

- From the recurring training card, I click "View Sessions"
- I see a filtered list of all sessions belonging to that template
- I can see status (Scheduled / Canceled / Completed) for each

### UC-5: Trainer cancels a single session

As a trainer, I want to cancel a specific session (e.g. holiday) without affecting the rest of the series.

- From the session detail, I click "Cancel" and provide a reason
- Only that session is canceled — other sessions remain scheduled
- Participants of that session are notified

### UC-6: Trainer completes a session

As a trainer, I want to mark a session as completed after it took place.

- From the session detail, I click "Complete"
- The session status changes to Completed

### UC-7: Trainer manages pending participants

As a trainer, I want to accept or reject participants who requested to join a session.

- On the session detail page, I see pending participants
- I can accept or reject each one individually

## Acceptance Criteria

- Session list page exists and is accessible from the main navigation
- Sessions can be filtered by date range
- Members can join/leave sessions directly from the list or detail
- Trainers can navigate from a recurring training to its generated sessions
- Session detail page shows participants, waitlist, and status
- Cancel and complete actions work for individual sessions

## Navigation

- Add "Sessions" (or merge with "Trainings" as a tab/toggle) to the main nav for all authenticated users
- Add "View Sessions" action on recurring training cards for trainers
