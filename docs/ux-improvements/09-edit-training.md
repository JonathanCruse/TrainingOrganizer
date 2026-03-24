# 09 — Edit Training

## Priority: High

## Problem

Once a training is created, there is no way to edit it in the UI. The API supports `PUT /api/v1/trainings/{id}` for updating title, description, dates, capacity, and visibility, but no edit dialog or form exists. If a trainer makes a mistake or needs to adjust details, they have no recourse within the app.

## Affected Roles

- **Trainer** — Needs to correct or adjust training details
- **Admin** — May need to adjust trainings on behalf of trainers

## Use Cases

### UC-1: Trainer corrects a typo in a draft training

As a trainer, I want to fix a typo in the title or description of a training I just created.

- On the training detail page, I click "Edit"
- A dialog or inline form opens with the current values pre-filled
- I change the title/description and save
- The training is updated immediately

### UC-2: Trainer changes the time of a published training

As a trainer, I want to reschedule a training to a different time.

- On the training detail, I edit the date/time fields
- If the training has a room booked, the booking is updated accordingly
- Participants should be aware of the change (or the system warns that participants exist)

### UC-3: Trainer adjusts capacity

As a trainer, I want to increase the max capacity of a training because more people want to join.

- I edit the max capacity from 15 to 20
- If there are waitlisted members, they are automatically promoted to fill the new spots
- The updated capacity is reflected on the training card

### UC-4: Trainer changes visibility

As a trainer, I want to change a training from MembersOnly to Public (or vice versa).

- I edit the visibility setting
- The change takes effect immediately
- The training appears/disappears from public view accordingly

## Acceptance Criteria

- Edit action is available on the training detail page
- Edit dialog/form is pre-filled with current training values
- Editable fields: title, description, start/end date+time, min/max capacity, visibility
- Editing is allowed in Draft and Published status (not Canceled/Completed)
- Changes are saved and reflected immediately
- If capacity is increased and waitlisted members exist, the UI communicates that promotions may happen
