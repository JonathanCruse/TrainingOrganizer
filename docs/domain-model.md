# TrainingOrganizer - Domain Model Design

## 1. Bounded Contexts

### 1.1 Membership Context
**Responsibility:** Manages member lifecycle — registration, profile, role assignment, approval workflows.

**Aggregates:** `Member`

### 1.2 Training Context
**Responsibility:** Manages training definitions, recurring training templates, and the generation of training sessions. Owns participation and attendance logic.

**Aggregates:** `Training`, `RecurringTraining`, `TrainingSession`

### 1.3 Facility Context
**Responsibility:** Manages physical locations, rooms, and room bookings. Enforces booking invariants (no double-booking).

**Aggregates:** `Location`, `Booking`

---

## 2. Aggregates

### 2.1 Member Aggregate (Membership Context)

**Purpose:** Represents a person registered in the system. Owns identity, profile, and role information.

**Aggregate Root:** `Member`

**Entities inside:** None (flat aggregate for fast reads and small document size).

**Value Objects:**
- `MemberId` (strongly typed ID wrapping `Guid`)
- `PersonName` { FirstName, LastName }
- `Email`
- `PhoneNumber`
- `ExternalIdentity` { Provider: string, SubjectId: string } — links to Keycloak
- `MemberRole` (enum: Guest, Member, Trainer, Admin)
- `RegistrationStatus` (enum: Pending, Approved, Rejected, Suspended)

**Invariants:**
- A member must have exactly one ExternalIdentity.
- Role transitions: Guest -> Member requires admin approval. Member -> Trainer requires admin assignment. Admin is assigned only by another Admin.
- A rejected or suspended member cannot participate in trainings.
- Email must be unique across all members (enforced via unique index, validated by domain service).

**Lifecycle:**
1. `Register(externalIdentity, name, email)` -> status = Pending, role = Guest
2. `Approve()` -> status = Approved, role = Member
3. `Reject(reason)` -> status = Rejected
4. `Suspend(reason)` -> status = Suspended
5. `Reinstate()` -> status = Approved
6. `AssignRole(role)` -> updates role (Admin action)
7. `UpdateProfile(name, email, phone)` -> updates profile data

### 2.2 Training Aggregate (Training Context)

**Purpose:** Represents a standalone (non-recurring) training definition. Owns its own participation list, waitlist, and capacity rules.

**Aggregate Root:** `Training`

**Entities inside:**
- `Participant` (entity within the aggregate, identified by MemberId)

**Value Objects:**
- `TrainingId`
- `TrainingTitle`
- `TrainingDescription`
- `TimeSlot` { Start: DateTimeOffset, End: DateTimeOffset }
- `Capacity` { Min: int, Max: int }
- `Visibility` (enum: Public, MembersOnly, InviteOnly)
- `TrainingStatus` (enum: Draft, Published, Canceled, Completed)
- `ParticipationStatus` (enum: Confirmed, Waitlisted, Canceled)
- `RoomRequirement` { RoomId, LocationId }

**Invariants:**
- Confirmed participant count must not exceed `Capacity.Max`.
- When capacity is full, new participants go to waitlist.
- When a confirmed participant cancels, the first waitlisted participant is auto-promoted.
- A training must have at least one assigned trainer.
- A canceled training cannot accept new participants.
- A member cannot be both confirmed and waitlisted simultaneously.
- Only assigned trainers or admins can modify the training.
- TimeSlot.End must be after TimeSlot.Start.
- Required rooms must be booked before the training is published (validated via domain service).

**Lifecycle:**
1. `Create(title, description, timeSlot, capacity, visibility, trainerIds)` -> status = Draft
2. `Publish()` -> status = Published (requires at least one trainer, valid time slot)
3. `Cancel(reason)` -> status = Canceled, all participants notified
4. `Complete()` -> status = Completed
5. `AddParticipant(memberId)` -> Confirmed or Waitlisted based on capacity
6. `RemoveParticipant(memberId)` -> promotes from waitlist if applicable
7. `AssignTrainer(memberId)` / `RemoveTrainer(memberId)`
8. `AddRoomRequirement(roomId, locationId)` / `RemoveRoomRequirement(roomId)`
9. `RecordAttendance(memberId, attended: bool)`

### 2.3 RecurringTraining Aggregate (Training Context)

**Purpose:** A template that defines recurrence rules and generates `TrainingSession` instances. Does NOT hold participants — those live on each generated session.

**Aggregate Root:** `RecurringTraining`

**Entities inside:** None.

**Value Objects:**
- `RecurringTrainingId`
- `RecurrenceRule` { Pattern: (Weekly/Biweekly/Monthly), DayOfWeek, TimeOfDay: TimeOnly, Duration: TimeSpan, StartDate, EndDate? }
- `TrainingTemplate` { Title, Description, Capacity, Visibility, TrainerIds, RoomRequirements }
- `RecurringTrainingStatus` (enum: Active, Paused, Ended)

**Invariants:**
- Recurrence rule must produce at least one future occurrence.
- EndDate (if set) must be after StartDate.
- Duration must be positive and reasonable (max 8 hours).
- Template must have at least one trainer.

**Lifecycle:**
1. `Create(template, recurrenceRule)` -> status = Active
2. `UpdateTemplate(template)` -> future unmodified sessions may be regenerated
3. `Pause()` -> stops generation of new sessions
4. `Resume()` -> resumes generation
5. `End()` -> status = Ended, no more sessions generated
6. `GenerateSessionsUntil(date)` -> produces domain events requesting session creation

### 2.4 TrainingSession Aggregate (Training Context)

**Purpose:** A concrete scheduled occurrence of a recurring training. Structurally identical to `Training` in behavior, but linked back to its `RecurringTrainingId`. Can override template properties.

**Aggregate Root:** `TrainingSession`

**Entities inside:**
- `Participant` (same structure as in Training)

**Value Objects:**
- `TrainingSessionId`
- `TimeSlot`
- `Capacity`
- `Visibility`
- `SessionStatus` (enum: Scheduled, Canceled, Completed)
- `ParticipationStatus`
- `RoomRequirement`
- `SessionOverrides` { Title?, Description?, Capacity?, TrainerIds?, RoomRequirements? }

**Invariants:**
- Same participation/capacity invariants as Training.
- A session can be individually canceled without affecting sibling sessions.
- Overridden properties take precedence over the template.

**Lifecycle:**
1. Created by system in response to `SessionsRequested` event from RecurringTraining
2. `Override(property, value)` -> marks property as overridden
3. `ResetToTemplate()` -> clears overrides
4. `Cancel(reason)` -> cancels this single session
5. `Complete()` -> marks as completed
6. Same participation methods as Training

### 2.5 Location Aggregate (Facility Context)

**Purpose:** Represents a physical venue. Owns its rooms as embedded entities.

**Aggregate Root:** `Location`

**Entities inside:**
- `Room` (identified by `RoomId`, embedded within the Location)

**Value Objects:**
- `LocationId`
- `RoomId`
- `Address` { Street, City, PostalCode, Country }
- `LocationName`
- `RoomName`
- `RoomCapacity` (int)
- `RoomStatus` (enum: Enabled, Disabled)

**Invariants:**
- Room names must be unique within a location.
- A disabled room cannot be booked for new trainings.
- A location must have at least a name and address.

**Lifecycle:**
1. `Create(name, address)`
2. `AddRoom(name, capacity)` -> adds room entity
3. `UpdateRoom(roomId, name?, capacity?)` -> modifies room
4. `EnableRoom(roomId)` / `DisableRoom(roomId)`
5. `UpdateAddress(address)`

### 2.6 Booking Aggregate (Facility Context)

**Purpose:** Represents a time-based reservation of a specific room. Enforces the "no double-booking" invariant.

**Aggregate Root:** `Booking`

**Entities inside:** None.

**Value Objects:**
- `BookingId`
- `TimeSlot` { Start, End }
- `BookingStatus` (enum: Active, Canceled)
- `BookingReference` { ReferenceType: (Training/TrainingSession/Manual), ReferenceId: string }

**Invariants:**
- No two active bookings for the same room may have overlapping time slots.
- A booking must reference the reason (training, session, or manual block).
- TimeSlot.End must be after TimeSlot.Start.

**Lifecycle:**
1. `Create(roomId, locationId, timeSlot, reference)` -> status = Active
2. `Cancel()` -> status = Canceled
3. `Reschedule(newTimeSlot)` -> validates no conflict exists for new slot

---

## 3. Entity Details

### 3.1 Participant (embedded in Training / TrainingSession)

| Property | Type |
|---|---|
| MemberId | MemberId (reference) |
| Status | ParticipationStatus |
| JoinedAt | DateTimeOffset |
| WaitlistPosition | int? (null if confirmed) |
| AttendanceRecorded | bool |
| Attended | bool |

**Behavior:**
- `Confirm()` -> sets status to Confirmed, clears waitlist position
- `Waitlist(position)` -> sets status to Waitlisted
- `Cancel()` -> sets status to Canceled
- `RecordAttendance(attended)` -> marks attendance

### 3.2 Room (embedded in Location)

| Property | Type |
|---|---|
| RoomId | RoomId |
| Name | RoomName |
| Capacity | int |
| Status | RoomStatus |

**Behavior:**
- `Enable()` / `Disable()`
- `UpdateDetails(name, capacity)`

---

## 4. Value Objects

| Value Object | Properties | Rules |
|---|---|---|
| `MemberId` | Guid | Immutable, non-empty |
| `TrainingId` | Guid | Immutable, non-empty |
| `TrainingSessionId` | Guid | Immutable, non-empty |
| `RecurringTrainingId` | Guid | Immutable, non-empty |
| `LocationId` | Guid | Immutable, non-empty |
| `RoomId` | Guid | Immutable, non-empty |
| `BookingId` | Guid | Immutable, non-empty |
| `PersonName` | FirstName, LastName | Both non-empty, max 100 chars |
| `Email` | Value: string | Must be valid email format |
| `PhoneNumber` | Value: string | Optional, validated format |
| `Address` | Street, City, PostalCode, Country | All non-empty |
| `TimeSlot` | Start, End | End > Start, both required |
| `Capacity` | Min, Max | Min >= 0, Max >= Min, Max > 0 |
| `RecurrenceRule` | Pattern, DayOfWeek, TimeOfDay, Duration, StartDate, EndDate? | Duration > 0, EndDate > StartDate if set |
| `ExternalIdentity` | Provider, SubjectId | Both non-empty |
| `BookingReference` | ReferenceType, ReferenceId | Both required |
| `RoomRequirement` | RoomId, LocationId | Both required |
| `TrainingTemplate` | Title, Description, Capacity, Visibility, TrainerIds, RoomRequirements | Title non-empty, at least one trainer |

---

## 5. Domain Services

### 5.1 `RoomBookingService`
- Checks for booking conflicts by querying Booking collection for overlapping active bookings.
- Creates Booking aggregates for each required room.
- Cancels bookings when a training or session is canceled.

### 5.2 `SessionGenerationService`
- Reads active RecurringTraining aggregates.
- Calculates next occurrence dates based on RecurrenceRule.
- Creates TrainingSession aggregates for each occurrence.
- Coordinates room booking via RoomBookingService.

### 5.3 `MemberUniquenessService`
- Queries the Member collection to check if an email is already in use.

---

## 6. Domain Events

| Event | Triggered When | Reacting Aggregates / Services |
|---|---|---|
| `MemberRegistered` | Member completes registration | Admin notification |
| `MemberApproved` | Admin approves a member | Notification to member |
| `MemberRejected` | Admin rejects a member | Notification to member |
| `MemberSuspended` | Admin suspends a member | Cancel active participations |
| `RoleAssigned` | Admin changes a member's role | — |
| `TrainingCreated` | New training is created | — |
| `TrainingPublished` | Training moves to Published | Create room bookings |
| `TrainingCanceled` | Training is canceled | Cancel bookings, notify participants |
| `TrainingCompleted` | Training is marked complete | — |
| `ParticipantJoined` | Member joins a training | Notification |
| `ParticipantCanceled` | Member cancels participation | Waitlist promotion |
| `ParticipantPromotedFromWaitlist` | Waitlisted member gets confirmed | Notification |
| `AttendanceRecorded` | Trainer records attendance | — |
| `RecurringTrainingCreated` | New recurring training template | Generate initial sessions |
| `RecurringTrainingPaused` | Template is paused | Stop generating sessions |
| `RecurringTrainingEnded` | Template is ended | — |
| `RecurringTrainingTemplateUpdated` | Template properties change | Update future sessions |
| `SessionsRequested` | RecurringTraining requests generation | Create TrainingSession aggregates |
| `TrainingSessionCanceled` | Individual session canceled | Cancel bookings |
| `RoomDisabled` | Room is disabled | Warn about future bookings |
| `BookingCreated` | Room is booked | — |
| `BookingCanceled` | Booking is canceled | — |

---

## 7. Aggregate Interaction Rules

### Reference Strategy
Aggregates reference each other **only by ID**, never by direct object reference.

| From | To | Reference Field |
|---|---|---|
| Training | Member | `trainerIds`, `participants[].memberId` |
| Training | Room/Location | `roomRequirements[].roomId/locationId` |
| TrainingSession | RecurringTraining | `recurringTrainingId` |
| Booking | Room/Location | `roomId`, `locationId` |
| Booking | Training/Session | `reference.id` |

### Acceptable Data Duplication
- TrainingSession duplicates effective title, description, capacity, trainer IDs from template.
- Booking stores locationId alongside roomId for query convenience.

---

## 8. Consistency Strategy

### Strong Consistency (within single aggregate)
- Participant count <= Capacity.Max
- No duplicate participants within a training
- Room name uniqueness within location
- Waitlist ordering and promotion
- Member status transitions

### Strong Consistency (via domain service + database)
- No overlapping bookings for same room (overlap query + unique index)
- Email uniqueness across members (unique index)

### Eventual Consistency (via domain events)
- Training canceled -> bookings canceled
- Member suspended -> participations canceled
- Recurring template updated -> future sessions updated
- Session generated -> rooms booked

---

## 9. MongoDB Document Examples

### Member
```json
{
  "_id": "member-uuid",
  "externalIdentity": { "provider": "keycloak", "subjectId": "kc-sub-123" },
  "name": { "firstName": "Jane", "lastName": "Doe" },
  "email": "jane@example.com",
  "phone": "+49 170 1234567",
  "role": "Member",
  "registrationStatus": "Approved",
  "registeredAt": "2026-01-15T10:00:00Z",
  "approvedAt": "2026-01-16T08:00:00Z",
  "approvedBy": "admin-member-uuid",
  "version": 3
}
```

### Training
```json
{
  "_id": "training-uuid",
  "title": "Advanced Kettlebell",
  "description": "...",
  "timeSlot": { "start": "2026-04-01T18:00:00+02:00", "end": "2026-04-01T19:30:00+02:00" },
  "capacity": { "min": 5, "max": 20 },
  "visibility": "MembersOnly",
  "status": "Published",
  "trainerIds": ["member-uuid-1"],
  "roomRequirements": [{ "roomId": "room-uuid-1", "locationId": "location-uuid-1" }],
  "participants": [
    { "memberId": "member-uuid-2", "status": "Confirmed", "joinedAt": "2026-03-20T10:00:00Z", "attendanceRecorded": true, "attended": true },
    { "memberId": "member-uuid-3", "status": "Waitlisted", "joinedAt": "2026-03-20T11:00:00Z", "waitlistPosition": 1, "attendanceRecorded": false, "attended": false }
  ],
  "createdAt": "2026-03-15T09:00:00Z",
  "createdBy": "member-uuid-1",
  "version": 7
}
```

### RecurringTraining
```json
{
  "_id": "recurring-uuid",
  "template": {
    "title": "Monday Evening Yoga",
    "description": "...",
    "capacity": { "min": 3, "max": 15 },
    "visibility": "Public",
    "trainerIds": ["member-uuid-1"],
    "roomRequirements": [{ "roomId": "room-uuid-2", "locationId": "location-uuid-1" }]
  },
  "recurrenceRule": { "pattern": "Weekly", "dayOfWeek": "Monday", "timeOfDay": "18:00", "duration": "01:30:00", "startDate": "2026-01-06", "endDate": null },
  "status": "Active",
  "lastGeneratedUntil": "2026-04-30",
  "createdAt": "2026-01-01T12:00:00Z",
  "createdBy": "member-uuid-1",
  "version": 2
}
```

### TrainingSession
```json
{
  "_id": "session-uuid",
  "recurringTrainingId": "recurring-uuid",
  "effectiveTitle": "Monday Evening Yoga",
  "timeSlot": { "start": "2026-04-06T18:00:00+02:00", "end": "2026-04-06T19:30:00+02:00" },
  "effectiveCapacity": { "min": 3, "max": 15 },
  "effectiveVisibility": "Public",
  "effectiveTrainerIds": ["member-uuid-1"],
  "effectiveRoomRequirements": [{ "roomId": "room-uuid-2", "locationId": "location-uuid-1" }],
  "overrides": {},
  "status": "Scheduled",
  "participants": [],
  "createdAt": "2026-03-01T00:00:00Z",
  "version": 1
}
```

### Location
```json
{
  "_id": "location-uuid-1",
  "name": "Downtown Fitness Center",
  "address": { "street": "Hauptstr. 12", "city": "Berlin", "postalCode": "10115", "country": "DE" },
  "rooms": [
    { "roomId": "room-uuid-1", "name": "Main Hall", "capacity": 30, "status": "Enabled" },
    { "roomId": "room-uuid-2", "name": "Yoga Studio", "capacity": 15, "status": "Enabled" }
  ],
  "createdAt": "2025-12-01T10:00:00Z",
  "version": 4
}
```

### Booking
```json
{
  "_id": "booking-uuid",
  "roomId": "room-uuid-1",
  "locationId": "location-uuid-1",
  "timeSlot": { "start": "2026-04-01T18:00:00+02:00", "end": "2026-04-01T19:30:00+02:00" },
  "status": "Active",
  "reference": { "type": "Training", "id": "training-uuid" },
  "createdAt": "2026-03-15T09:00:00Z",
  "createdBy": "member-uuid-1",
  "version": 1
}
```

---

## 10. Key Business Rules

1. **Room booking conflicts:** Two active bookings for the same roomId must not overlap. Check: `existing.start < new.end AND existing.end > new.start`.
2. **Capacity limits:** Confirmed participants <= Capacity.Max. Overflow goes to waitlist. Auto-promote on cancellation.
3. **Trainer assignment:** Only members with Trainer/Admin role. At least one trainer required to publish.
4. **Recurring generation:** Rolling window (e.g., 4 weeks ahead). Idempotent via `lastGeneratedUntil`. Paused/ended templates stop generating.
5. **Member participation:** Suspended/rejected members cannot join. No overlapping training times for same member.
6. **Training lifecycle:** Draft -> Published -> Canceled|Completed. Terminal states are final.

---

## Recommended MongoDB Indexes

```
members:           { "externalIdentity.subjectId": 1 } unique
                   { "email": 1 } unique

trainings:         { "status": 1, "timeSlot.start": 1 }
                   { "trainerIds": 1 }
                   { "participants.memberId": 1 }

training_sessions: { "recurringTrainingId": 1, "timeSlot.start": 1 }
                   { "status": 1, "timeSlot.start": 1 }
                   { "participants.memberId": 1 }

bookings:          { "roomId": 1, "status": 1, "timeSlot.start": 1, "timeSlot.end": 1 }
                   { "reference.type": 1, "reference.id": 1 }
```
