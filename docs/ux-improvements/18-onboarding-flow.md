# 18 — New Member Onboarding

## Priority: Low

## Problem

When a new member registers, they enter a "Pending" state and have to wait for admin approval. During this time, there's no guidance on what's happening, how long it might take, or what they can do once approved. The experience after approval is equally bare — no introduction to the app's features.

## Affected Roles

- **New Member** — Needs guidance through the registration and waiting process

## Use Cases

### UC-1: New member understands the approval process

As a new member who just registered, I want to understand what happens next.

- After registration, I see a clear status page or banner
- It explains: "Your registration is being reviewed. An administrator will approve your account."
- I understand this is normal and that I need to wait

### UC-2: New member sees their status while waiting

As a pending member, each time I log in, I want to see my current status.

- The app shows a prominent banner: "Your account is pending approval"
- I can't access training features yet, and this is clearly communicated (not just empty pages)
- I don't see confusing empty lists — I see a purpose-built waiting state

### UC-3: Newly approved member discovers features

As a freshly approved member, I want to know what I can do now.

- After my first login as an approved member, I see a welcome message or brief tour
- Key actions are highlighted: "Browse Trainings", "View Schedule", "Update Profile"
- This could be a simple welcome card on the dashboard that dismisses after first viewing

### UC-4: Rejected member understands what happened

As a rejected member, I want to understand that my application was not accepted.

- I see a clear message: "Your registration was not approved"
- If a reason was provided, I see it
- I understand there's nothing more I can do in the app

## Acceptance Criteria

- Pending members see a clear status indicator on every page (banner or dedicated status page)
- Pending members are gracefully blocked from member features (not via confusing empty states)
- Newly approved members receive a welcome/orientation experience
- Rejected members see a clear, empathetic rejection message with reason if provided
- The onboarding flow works on both web and mobile
