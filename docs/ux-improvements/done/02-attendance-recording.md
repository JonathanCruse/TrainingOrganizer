# 02 — Attendance Recording

## Priority: Critical

## Problem

After a training or session takes place, trainers have no way to record who actually showed up. The backend supports attendance tracking, but there is no UI for it. Without attendance data, the club cannot track participation patterns, enforce minimum attendance, or generate reports.

## Affected Roles

- **Trainer** — Needs to record attendance after each training/session
- **Member** — Wants to see their own attendance history
- **Admin** — Needs attendance data for reporting and member management

## Use Cases

### UC-1: Trainer records attendance after a training

As a trainer, I want to quickly mark which participants attended a training so attendance is tracked.

- After a training/session has taken place, I open the detail page
- I see a list of confirmed participants with checkboxes
- I check off who attended, leave unchecked who didn't
- I click "Save Attendance"
- The system records attendance for all participants in one action

### UC-2: Trainer records attendance on the go

As a trainer, I want to record attendance right after the session ends, ideally from my phone.

- The attendance UI works well on mobile screens
- A simple checklist with large tap targets
- Quick save without navigating away

### UC-3: Member views their own attendance history

As a member, I want to see which trainings I attended so I can track my own consistency.

- On my profile or schedule page, I see a history of past trainings
- Each entry shows whether I attended or not
- I can see a summary (e.g. "Attended 12 of 15 sessions this month")

### UC-4: Trainer adjusts attendance after the fact

As a trainer, I want to correct an attendance entry if I made a mistake.

- I can revisit a completed training and update attendance records
- Changes are saved without needing to re-enter everything

## Acceptance Criteria

- Attendance recording UI appears on training/session detail for trainers
- Trainers can mark attendance for all confirmed participants at once
- Attendance is only recordable for Published/Completed trainings (not Draft/Canceled)
- Works well on mobile (large checkboxes, clear layout)
- Members can see their own attendance status on past trainings
