# 07 — Schedule Calendar View

## Priority: High

## Problem

The "My Schedule" page shows a flat table of upcoming events for the next 30 days. This is hard to scan visually — members can't quickly see which days they're busy, spot gaps, or get a sense of their weekly rhythm. A calendar or timeline view would be far more intuitive.

## Affected Roles

- **Member** — Wants to see their training schedule at a glance
- **Trainer** — Wants to see their teaching schedule across the week

## Use Cases

### UC-1: Member views their weekly schedule

As a member, I want to see my upcoming trainings in a weekly view so I can plan around them.

- I open "My Schedule"
- I see a week view with my trainings placed on the correct days and times
- I can navigate forward/backward by week
- Clicking on an entry takes me to the training/session detail

### UC-2: Member views their monthly overview

As a member, I want to see a monthly overview to spot patterns and plan ahead.

- I switch to a month view
- Days with trainings are highlighted or marked
- I can see at a glance which days I have something scheduled

### UC-3: Member sees today's trainings prominently

As a member, I want to immediately see what's happening today when I open my schedule.

- Today is highlighted in the calendar
- Today's trainings are shown prominently (top of page or expanded)
- If nothing is scheduled today, I see "No trainings today"

### UC-4: Trainer views their teaching schedule

As a trainer, I want to see all sessions I'm teaching across the week so I can prepare.

- My schedule includes both trainings I participate in and those I lead
- Trainings where I'm the trainer are visually distinct (different color, icon, or label)
- I can quickly see my teaching load for the week

## Acceptance Criteria

- Schedule page offers at least a week view (month view is nice-to-have)
- Trainings are shown as blocks on the appropriate day/time
- Navigation to go forward/backward in time
- Today is visually highlighted
- Clicking a training block navigates to its detail page
- Trainer's own sessions are visually distinguishable
- Works reasonably on mobile (day view fallback is acceptable)
