# 11 — Confirmation Dialogs for Destructive Actions

## Priority: Medium

## Problem

Destructive actions like canceling a training, suspending a member, or rejecting a participant fire immediately on button click with no confirmation. A misclick can cancel a training with 20 registered participants or suspend a member accidentally. The impact of these actions is significant and often irreversible.

## Affected Roles

- **Trainer** — Cancels trainings, rejects participants
- **Admin** — Suspends/rejects members, cancels trainings

## Use Cases

### UC-1: Trainer cancels a training

As a trainer, when I click "Cancel Training", I want to see a confirmation showing the impact before it actually happens.

- I click "Cancel Training"
- A dialog appears: "Cancel [Training Title]? This will notify [N] confirmed participants and [M] waitlisted members. This action cannot be undone."
- I must provide a cancellation reason
- I confirm or go back
- Only after confirmation does the cancellation happen

### UC-2: Admin suspends a member

As an admin, when I click "Suspend", I want to understand what will happen.

- I click "Suspend"
- A dialog appears: "Suspend [Member Name]? They will be removed from all active training registrations. This can be reversed by reinstating."
- I provide a reason
- I confirm or cancel

### UC-3: Admin rejects a pending member

As an admin, when I click "Reject", I want to confirm my decision.

- A dialog asks for confirmation and a rejection reason
- I understand this is a terminal state for the member's application

### UC-4: Trainer rejects a participant

As a trainer, when I reject a participant from a training, I want to confirm.

- A brief confirmation: "Reject [Member Name] from [Training Title]?"
- Confirm or cancel

## Acceptance Criteria

- All destructive actions require confirmation via dialog
- Confirmation dialogs clearly state what will happen and the impact
- Actions requiring a reason (cancel training, suspend, reject) collect the reason in the dialog
- The dialog has a clear "Cancel" option to back out
- Non-destructive actions (join, publish, approve) do NOT require confirmation — keep them quick
