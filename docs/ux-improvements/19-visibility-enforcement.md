# 19 — Training Visibility Enforcement in UI

## Priority: Low

## Problem

Trainings have a visibility setting (Public, MembersOnly, InviteOnly) but the UI treats all trainings the same. All trainings appear in the same list regardless of visibility. There's no visual distinction, no filtering by visibility, and no enforcement of who can see or join restricted trainings.

## Affected Roles

- **Member** — Should only see trainings they're allowed to join
- **Trainer** — Needs to understand how visibility affects their training's audience

## Use Cases

### UC-1: Member sees only relevant trainings

As a member, I only want to see trainings I'm eligible to join.

- Public trainings are visible to everyone (including unauthenticated users browsing)
- MembersOnly trainings are visible only to approved members
- InviteOnly trainings are visible only to invited members (or not shown in list at all)
- The list naturally filters based on my status and role

### UC-2: Member identifies restricted trainings

As a member, when I see a training that has restricted visibility, I want a visual indicator.

- MembersOnly trainings show a subtle badge or icon (e.g. a members-only label)
- InviteOnly trainings show an invite-only indicator
- Public trainings have no special marking (they're the default)

### UC-3: Trainer sets visibility with understanding

As a trainer creating a training, I want to understand what each visibility option means.

- The visibility dropdown in the create dialog has descriptions:
  - Public: "Visible to everyone, anyone can join"
  - MembersOnly: "Only approved members can see and join"
  - InviteOnly: "Only invited members can join"
- I choose the appropriate option based on the training type

### UC-4: Unauthenticated user sees only public trainings

As a visitor who hasn't logged in, I should only see public trainings on the training list.

- The training list (which is accessible without login) only shows Public trainings
- This serves as a teaser to encourage registration

## Acceptance Criteria

- Training list filters by visibility based on the viewer's authentication and role
- Visual indicators (icons, badges, or labels) distinguish visibility levels
- Visibility dropdown in create/edit dialogs includes descriptions
- Unauthenticated users only see Public trainings
- InviteOnly trainings are hidden from the general list (shown only to invited members)
