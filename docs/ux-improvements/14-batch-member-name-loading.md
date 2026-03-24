# 14 — Batch Member Name Loading

## Priority: Medium

## Problem

The training detail page loads participant names one by one — each participant triggers a separate `GetByIdAsync` API call. For a training with 20 participants, that's 20 sequential HTTP requests. This causes visible loading delays and a flickering UI as names pop in one at a time.

## Affected Roles

- **Member** — Sees slow-loading participant lists
- **Trainer** — Waits for all names to load before managing participants

## Use Cases

### UC-1: Member opens a training with many participants

As a member, when I open a training detail page, I want to see all participant names load quickly — not trickle in one by one.

- I open a training detail page
- All participant names appear together within a reasonable time
- No visible flickering or sequential loading

### UC-2: Trainer reviews participants for approval

As a trainer, I need to see all pending participants at once to make approval decisions.

- The participant list loads completely in one go
- I don't have to wait for names to appear before I can act

## Possible Approaches

- **Batch endpoint:** A new API endpoint that accepts multiple member IDs and returns all names at once
- **Embed names in training response:** Include participant names directly in the training response from the backend
- **Client-side cache:** Cache member names after first load to avoid repeated calls

## Acceptance Criteria

- Training detail page loads participant names in one batch, not individually
- Visible loading time for participant names is under 1 second for typical trainings (5-30 participants)
- No flickering or sequential name appearance
- Works for both confirmed participants and waitlist
