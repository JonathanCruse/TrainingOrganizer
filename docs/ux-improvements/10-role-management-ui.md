# 10 — Role Management UI

## Priority: High

## Problem

Admins can assign and remove roles via the API (`POST /{id}/roles`, `DELETE /{id}/roles/{role}`), but there is no UI for it. To promote a member to Trainer or Admin, the admin has to use an external tool or API call directly. This is a key admin workflow that's missing from the app.

## Affected Roles

- **Admin** — Needs to manage member roles

## Use Cases

### UC-1: Admin promotes a member to trainer

As an admin, I want to assign the Trainer role to a member so they can create and manage trainings.

- On the member list or member detail, I see the member's current roles
- I click "Assign Role" or a similar action
- I select "Trainer" from the available roles
- The role is added — the member now has both Member and Trainer roles
- The role change is reflected immediately in the UI

### UC-2: Admin promotes a member to admin

As an admin, I want to give another member admin privileges so they can help manage the club.

- Same flow as UC-1, but selecting "Admin"
- The system may show a confirmation since this grants full access

### UC-3: Admin removes a role from a member

As an admin, I want to remove the Trainer role from a member who is no longer teaching.

- On the member's role display, I click a remove/x button on the Trainer role chip
- The role is removed
- The member retains their Member role

### UC-4: Admin reviews all trainers

As an admin, I want to see which members have the Trainer role so I can manage the trainer roster.

- I filter the member list by role: "Trainer"
- I see all trainers and can manage their roles from there

## Acceptance Criteria

- Role management actions are available on the member list (inline or via detail page)
- Admin can assign any role: Member, Trainer, Admin
- Admin can remove roles (with the constraint that at least one role remains)
- Role changes are reflected immediately (role chips update)
- Confirmation dialog when assigning Admin role
- Role filter on the member list (see also #06)
