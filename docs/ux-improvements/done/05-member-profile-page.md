# 05 — Member Profile Page

## Priority: Critical

## Problem

Members have no way to view or edit their own profile in the UI. The API supports `GET /me` and `PUT /me`, but there is no page for it. Members cannot see their own status, roles, email, or update their contact information.

## Affected Roles

- **Member** — Needs to view and manage their own profile
- **Trainer** — Same, plus wants to see their trainer-specific info
- **Admin** — Same, plus may want to view other members' profiles

## Use Cases

### UC-1: Member views their profile

As a member, I want to see my profile information so I know what the club has on file for me.

- I click on my name or a "Profile" link in the navigation or user menu
- I see: name, email, phone, roles, registration status, registration date
- I see my current roles displayed clearly (Member, Trainer, Admin)

### UC-2: Member updates their contact info

As a member, I want to update my name, email, or phone number when they change.

- On my profile page, I click "Edit" or directly modify fields
- I change my name, email, or phone
- I save and see the updated information

### UC-3: Member sees their membership status

As a member, I want to understand where I am in the approval process.

- If pending: I see "Awaiting Approval" with an explanation
- If approved: I see "Active Member" with my roles
- If suspended: I see "Suspended" with the reason (if provided)
- If rejected: I see "Rejected" with the reason

### UC-4: Member views their training history

As a member, I want to see a history of trainings I've participated in.

- On my profile or a linked page, I see past trainings
- Each entry shows: title, date, whether I attended
- This gives me a sense of my activity in the club

## Acceptance Criteria

- Profile page accessible from the user menu (top-right) for all authenticated users
- Displays all member info: name, email, phone, roles, status, registration date
- Edit mode allows updating name, email, phone
- Membership status is clearly communicated with appropriate messaging
- Works on both web and mobile
