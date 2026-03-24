# 03 — Room Selection in Training Creation

## Priority: Critical

## Problem

When a trainer creates a training or recurring training, there is no way to select a location and room. The domain model supports room requirements and booking, but the creation dialogs don't expose this. Trainings are created without room assignments, making facility coordination happen outside the app.

## Affected Roles

- **Trainer** — Needs to specify where the training takes place
- **Admin** — Needs room bookings to prevent conflicts
- **Member** — Wants to know where to go

## Use Cases

### UC-1: Trainer selects a room when creating a training

As a trainer, I want to pick a location and room when creating a training so participants know where to go and the room is reserved.

- In the "New Training" dialog, I see a "Location & Room" section
- I select a location from a dropdown (shows all locations)
- After selecting a location, I see its available rooms
- I pick a room — the system shows room capacity so I can match it to training capacity
- When I publish the training, the room is automatically booked for the timeslot

### UC-2: Trainer sees room availability

As a trainer, I want to know if a room is available for my chosen timeslot before committing.

- After selecting a room and timeslot, I see a quick availability indicator
- If the room is already booked, I'm warned and can choose a different room or time
- I don't have to guess or check a separate calendar

### UC-3: Trainer selects a room for a recurring training

As a trainer, I want to assign a default room to a recurring training so all generated sessions are booked in that room.

- In the "New Recurring Training" dialog, I select a location and room
- Generated sessions inherit this room assignment
- If a conflict exists for a specific occurrence, it's flagged during generation

### UC-4: Trainer changes the room for an existing training

As a trainer, I want to move a training to a different room if the original one becomes unavailable.

- On the training detail, I can change the assigned room
- The old booking is canceled and a new one is created
- Participants see the updated location

## Acceptance Criteria

- Location and room dropdowns appear in both create training and create recurring training dialogs
- Room selection is optional (not all trainings need a room)
- Room capacity is visible during selection to help trainers choose appropriately
- Room booking happens automatically when the training is published
- Conflict warnings are shown if the chosen room is already booked
