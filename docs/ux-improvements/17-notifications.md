# 17 — Notification System

## Priority: Low

## Problem

Important events happen silently — a member gets promoted from waitlist, a training is canceled, a member's registration is approved. There is no in-app notification mechanism to inform users about events that happened while they weren't looking at the relevant page. Members have to manually check each training to see if anything changed.

## Affected Roles

- **Member** — Needs to know about waitlist promotions, cancellations, approval status
- **Trainer** — Needs to know about new participant requests, member cancellations
- **Admin** — Needs to know about new member registrations

## Use Cases

### UC-1: Member is notified of waitlist promotion

As a member, when I'm promoted from the waitlist to a confirmed spot, I want to know about it.

- I see a notification (bell icon badge, toast, or notification panel)
- The notification says: "You've been confirmed for [Training Title] on [Date]"
- Clicking it takes me to the training detail

### UC-2: Member is notified of training cancellation

As a member, when a training I joined is canceled, I want to know immediately.

- Notification: "[Training Title] on [Date] has been canceled. Reason: [reason]"
- I don't have to discover this by checking my schedule

### UC-3: Member is notified of approval

As a new member, when my registration is approved, I want to know so I can start using the app.

- Notification: "Your membership has been approved! You can now join trainings."

### UC-4: Trainer is notified of new participant requests

As a trainer, when someone requests to join my training, I want to know so I can act on it.

- Notification: "[Member Name] requested to join [Training Title]"
- Clicking it takes me to the training detail where I can approve/reject

### UC-5: Admin is notified of new registrations

As an admin, when a new member registers, I want to know so I can review their application.

- Notification: "New member registration: [Name]"
- Badge on the notification bell shows unread count

## Acceptance Criteria

- Notification bell icon in the top app bar with unread count badge
- Notification panel/dropdown showing recent notifications
- Notifications are generated for key domain events:
  - Waitlist promotion
  - Training/session cancellation
  - Member approval/rejection
  - New participant requests (for trainers)
  - New registrations (for admins)
- Clicking a notification navigates to the relevant page
- Notifications are marked as read when viewed
- Works on mobile (potentially with push notifications in the future)
