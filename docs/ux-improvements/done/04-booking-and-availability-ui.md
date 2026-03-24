# 04 — Booking & Room Availability UI

## Priority: Critical

## Problem

The system has a full booking API (create, cancel, reschedule, check availability) but zero UI for it. Trainers and admins cannot see which rooms are booked when, check availability, or manage bookings. Room scheduling happens blind.

## Affected Roles

- **Trainer** — Needs to see room availability when planning trainings
- **Admin** — Needs oversight of all room bookings across locations

## Use Cases

### UC-1: Admin views all bookings for a location

As an admin, I want to see all room bookings for a location so I can understand facility utilization.

- I navigate to a location detail or booking overview page
- I see a timeline or calendar view showing bookings per room
- Each booking shows: time, training title, status (Active/Canceled)
- I can filter by date range and room

### UC-2: Trainer checks room availability

As a trainer, I want to check if a room is free on a specific date/time before planning a training.

- I navigate to a room's availability view
- I select a date range
- I see which timeslots are free vs. booked
- I can see who/what has booked occupied slots

### UC-3: Admin creates a manual booking

As an admin, I want to book a room for a non-training event (e.g. club meeting, maintenance).

- I create a booking specifying room, date/time, and a reference/description
- The booking shows up alongside training bookings
- Trainers see the room as unavailable for that slot

### UC-4: Admin cancels a booking

As an admin, I want to cancel a room booking if plans change.

- From the booking view, I cancel a specific booking
- The room becomes available again for that timeslot

### UC-5: Admin reschedules a booking

As an admin, I want to move a booking to a different time without canceling and recreating.

- I select a booking and change the timeslot
- The system checks for conflicts with the new time
- The booking is updated in place

## Acceptance Criteria

- A booking overview page exists, accessible from the Locations section or as a separate nav item
- Bookings are displayed visually (calendar, timeline, or table grouped by day)
- Room availability can be checked for any date range
- Manual bookings can be created for non-training events
- Cancel and reschedule actions work from the UI
- Training-linked bookings show the associated training name
