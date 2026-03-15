# TrainingOrganizer — Architecture & Development Blueprint

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Bounded Contexts](#2-bounded-contexts)
3. [Domain Model](#3-domain-model)
4. [Aggregates](#4-aggregates)
5. [Domain Events](#5-domain-events)
6. [API Design](#6-api-design)
7. [MongoDB Schema Strategy](#7-mongodb-schema-strategy)
8. [Clean Architecture Project Structure](#8-clean-architecture-project-structure)
9. [Frontend Architecture](#9-frontend-architecture)
10. [Kubernetes Deployment Architecture](#10-kubernetes-deployment-architecture)
11. [CI/CD Pipeline Design](#11-cicd-pipeline-design)
12. [Observability Strategy](#12-observability-strategy)
13. [Scalability Considerations](#13-scalability-considerations)
14. [Future Mobile Support Strategy](#14-future-mobile-support-strategy)

---

## 1. System Overview

### What the system does

TrainingOrganizer is a training management platform for organizations that run scheduled fitness/education trainings across multiple physical locations. It manages the full lifecycle: member registration and approval, training creation and scheduling, recurring training automation, room booking with conflict prevention, participation with waitlists, attendance tracking, and reporting.

### Architecture Decision: Modular Monolith

**Recommendation: Start as a modular monolith, extract to microservices only when forced by operational needs.**

#### Why not microservices from day one?

| Factor | Microservices | Modular Monolith |
|--------|--------------|-------------------|
| Team size (1-3 devs) | Massive overhead | Right-sized |
| Domain boundaries settled? | Need to be stable | Can refactor cheaply |
| Data consistency | Saga complexity | In-process events, shared DB |
| Deployment complexity | K8s + service mesh + distributed tracing from day 1 | Single container, trivially deployable |
| Latency | Network hops between services | In-process calls |
| Debugging | Distributed trace correlation | Stack traces |

This system has 3 bounded contexts with clear inter-aggregate event flows. The membership context is nearly autonomous. Training and Facility interact through booking events but share no aggregates. These boundaries are clean enough that extraction is straightforward *when the time comes*.

#### When to extract

- **Notification service** — first candidate. Stateless, high I/O, different scaling profile. Extract when email/push volume justifies dedicated workers.
- **Reporting/analytics** — second candidate. Read-heavy, tolerates staleness, benefits from separate read models or a data warehouse.
- **Scheduling automation** — third candidate. Background processing (session generation, waitlist promotion, reminders) can become a worker service when volume grows.

#### Modular monolith structure

```
TrainingOrganizer (single deployable)
├── Modules/
│   ├── Membership/         ← bounded context
│   ├── Training/           ← bounded context
│   ├── Facility/           ← bounded context
│   ├── Scheduling/         ← cross-cutting automation
│   ├── Notification/       ← cross-cutting notifications
│   └── Reporting/          ← cross-cutting read models
├── SharedKernel/           ← base classes, TimeSlot, Guard
└── Host/                   ← ASP.NET entry point, composition root
```

Modules communicate only through:
1. **Domain events** (in-process via MediatR/in-memory bus)
2. **Strongly-typed IDs** (never object references)
3. **Query interfaces** (read-only cross-module queries where events are insufficient)

This preserves microservice-ready boundaries without distributed system complexity.

### High-Level Component Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Clients                                     │
│   Web App (React)  ·  Mobile App (React Native)  ·  Admin Panel     │
└──────────────────────────────┬──────────────────────────────────────┘
                               │ HTTPS / REST + OpenID Connect
                               ▼
┌──────────────────────────────────────────────────────────────────────┐
│                    ASP.NET Core Web API                               │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌───────────────────┐   │
│  │Membership│  │ Training │  │ Facility │  │ Scheduling/Notif. │   │
│  │  Module   │  │  Module  │  │  Module  │  │     Workers       │   │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────────┬──────────┘   │
│       │              │             │                  │              │
│       └──────────────┴─────────────┴──────────────────┘              │
│                    In-Process Event Bus (MediatR)                     │
└──────────────────────────────┬───────────────────────────────────────┘
                               │
              ┌────────────────┼────────────────┐
              ▼                ▼                ▼
      ┌──────────────┐  ┌──────────┐  ┌──────────────────┐
      │   MongoDB     │  │ Keycloak │  │ Email/Push       │
      │   (primary    │  │  (IdP)   │  │ (SMTP/Firebase)  │
      │    store)     │  │          │  │                   │
      └──────────────┘  └──────────┘  └──────────────────┘
```

---

## 2. Bounded Contexts

### 2.1 Membership Context

**Responsibility:** Member lifecycle — registration, identity linking, profile management, role assignment, approval workflows.

**Aggregate:** `Member`

**External dependencies:** Keycloak (via ExternalIdentity value object). The domain stores only a reference (provider + subjectId); all authentication flows are handled by Keycloak.

**Key invariant:** Email uniqueness across all members (enforced by `IMemberUniquenessService` + MongoDB unique index).

**Outbound events:** `MemberRegistered`, `MemberApproved`, `MemberRejected`, `MemberSuspended`, `RoleAssigned`, `RoleRemoved`

**Consumers of these events:**
- Training context — suspending a member cancels their active participations
- Notification module — sends emails on approval/rejection/suspension
- Reporting module — membership statistics

### 2.2 Training Context

**Responsibility:** Training definitions, recurring templates, session generation, participation management (join/leave/waitlist/attendance).

**Aggregates:**
- `Training` — standalone one-off training with embedded participants
- `RecurringTraining` — template that produces sessions on a schedule
- `TrainingSession` — generated occurrence with embedded participants, can override template properties

**Key interaction:** `RecurringTraining.GenerateSessionsUntil()` raises `SessionsRequestedEvent` → `ISessionGenerationService` creates `TrainingSession` aggregates and coordinates room booking via `IRoomBookingService`.

**Key invariants:**
- Confirmed participants <= Capacity.Max (auto-waitlist overflow, auto-promote on cancellation)
- At least one trainer required to publish
- Terminal states (Canceled, Completed) are final

**Outbound events:** 14 events covering training lifecycle, participation changes, session generation

**Consumers:**
- Facility context — training published/canceled → create/cancel room bookings
- Notification module — participant joined/promoted/canceled, training canceled
- Scheduling module — session generation triggers, reminder scheduling

### 2.3 Facility Context

**Responsibility:** Physical locations, rooms, and time-based room bookings.

**Aggregates:**
- `Location` — venue with embedded Room entities
- `Booking` — time-slot reservation for a specific room

**Key invariant:** No two active bookings for the same room may have overlapping time slots. Enforced by `IRoomBookingService` (overlap query + MongoDB partial unique index on `{roomId, timeSlot.start, timeSlot.end}` where status=Active).

**Outbound events:** `BookingCreated`, `BookingCanceled`, `RoomDisabled`

**Consumers:**
- Training context — room disabled → warn about affected trainings
- Scheduling module — calendar/availability views

### 2.4 Cross-Cutting Modules (Application Layer)

These are not bounded contexts (no domain aggregates) but application-layer modules:

**Notification Module**
- Consumes domain events and dispatches notifications
- Supports email (SMTP), in-app (WebSocket/SignalR), push (Firebase)
- Template-based rendering (training name, member name, etc.)
- Delivery preferences per member

**Scheduling Module**
- Background workers for:
  - Recurring session generation (rolling window, e.g., 4 weeks ahead)
  - Training reminders (24h before, configurable)
  - Auto-cancel empty trainings (optional, below Capacity.Min)
  - Waitlist promotion processing
- Uses `IHostedService` / `BackgroundService` in .NET

**Reporting Module**
- Read-only projections optimized for queries
- Room utilization, participation rates, trainer workload, attendance statistics
- Can use separate MongoDB collections (materialized views) or direct aggregation pipelines

### Context Map

```
  Membership ──── (events) ────► Training
       │                            │
       │                            │ (events: publish/cancel → book/unbook rooms)
       │                            ▼
       └──── (events) ──────► Facility
                                    │
    Notification ◄── (events) ──────┤
    Scheduling   ◄── (events) ──────┘
    Reporting    ◄── (events) ──── (all three contexts)
```

**Integration patterns:**
- Membership → Training: **Conformist** (Training accepts Member IDs as-is)
- Training → Facility: **Customer-Supplier** (Training requests bookings, Facility owns booking logic)
- All → Notification: **Published Language** (events are the shared language)

---

## 3. Domain Model

The domain layer is fully implemented. See `docs/domain-model.md` for the complete specification. Below is a summary of what exists.

### Base Classes (`Domain/Common/`)

| Class | Purpose |
|-------|---------|
| `ValueObject` | Abstract base record for value objects (structural equality) |
| `StronglyTypedId` | Abstract base record wrapping `Guid` |
| `Entity<TId>` | Abstract base class with required ID, equality by ID |
| `AggregateRoot<TId>` | Extends Entity, manages domain event list and version counter |
| `IDomainEvent` | Interface with `DateTimeOffset OccurredAt` |
| `Guard` | Static validation methods (null checks, format validation, range checks) |
| `TimeSlot` | Shared value object: `(DateTimeOffset Start, DateTimeOffset End)` with `OverlapsWith()` |

### Exception Hierarchy (`Domain/Exceptions/`)

| Exception | When |
|-----------|------|
| `DomainException` | Base for all domain errors |
| `BusinessRuleViolationException` | Invariant violated (capacity exceeded, invalid transition) |
| `EntityNotFoundException` | Aggregate/entity not found by ID |
| `InvalidEntityStateException` | Operation attempted in wrong state |

### Code Conventions (already enforced)

- **No `= null!`** — `required` keyword + `init` accessors, mutable via backing field
- `[SetsRequiredMembers]` on private constructors
- CS8618 suppressed project-wide
- Sealed records for value objects and events
- Static factory methods on aggregates (no public constructors)
- Enums for state machines with guarded transitions

---

## 4. Aggregates

### Summary Table

| Aggregate | Context | Embedded Entities | Key Invariants | MongoDB Collection |
|-----------|---------|-------------------|----------------|--------------------|
| `Member` | Membership | — | Email unique, valid state transitions | `members` |
| `Training` | Training | `Participant` (list) | Capacity, waitlist auto-promote, min 1 trainer | `trainings` |
| `RecurringTraining` | Training | — | Valid recurrence rule, template has trainer | `recurring_trainings` |
| `TrainingSession` | Training | `Participant` (list) | Same as Training + linked to template | `training_sessions` |
| `Location` | Facility | `Room` (list) | Room name unique within location | `locations` |
| `Booking` | Facility | — | No overlapping active bookings per room | `bookings` |

### Aggregate Design Rationale

**Why Participant is embedded (not a separate aggregate):**
The capacity invariant (`confirmedCount <= max`) must be strongly consistent. If Participant were a separate aggregate, enforcing this would require distributed locking or saga patterns. Embedding participants means the Training aggregate can atomically validate capacity in a single MongoDB document update.

**Why Room is embedded in Location:**
Rooms have no independent lifecycle. They're always created, queried, and managed through their location. Embedding avoids the need for cross-aggregate consistency between Location and Room.

**Why Booking is a separate aggregate (not embedded in Location or Training):**
The double-booking invariant spans *all* bookings across *all* trainings for a given room. This requires querying the full set of bookings, which is best done as a collection-level operation. Embedding bookings in Location would cause unbounded document growth.

**Why RecurringTraining is separate from TrainingSession:**
A recurring template potentially generates hundreds of sessions over its lifetime. Embedding them would create unbounded document growth. Separation also allows individual session modification without touching the template.

### ParticipantManager (Internal Helper)

`Training` and `TrainingSession` share identical participation logic (add/remove/waitlist/promote/attendance). Rather than duplicate this code, `ParticipantManager` is an internal class that encapsulates the algorithm:

1. `AddParticipant(memberId, capacity)` — confirm if space, else waitlist
2. `RemoveParticipant(memberId)` — cancel and auto-promote first waitlisted
3. `RecordAttendance(memberId, attended)` — only for confirmed participants

Both aggregates delegate to ParticipantManager, which operates on their participant list.

---

## 5. Domain Events

### Event Flow Architecture

Domain events are raised within aggregates (via `AddDomainEvent()` on `AggregateRoot`) and dispatched after the aggregate is persisted. This ensures events are only published for committed state changes.

**Dispatch strategy:** After `SaveChangesAsync`, the infrastructure layer collects domain events from the persisted aggregate and publishes them through MediatR's `INotificationHandler<T>`. This is in-process and synchronous (within the same request for critical handlers) or queued (for non-critical handlers like notifications).

### Complete Event Catalog

#### Membership Events

| Event | Payload | Raised By | Typical Handlers |
|-------|---------|-----------|------------------|
| `MemberRegisteredEvent` | MemberId, Email | `Member.Register()` | Admin notification |
| `MemberApprovedEvent` | MemberId, ApprovedBy | `Member.Approve()` | Member notification |
| `MemberRejectedEvent` | MemberId, Reason | `Member.Reject()` | Member notification |
| `MemberSuspendedEvent` | MemberId, Reason | `Member.Suspend()` | Cancel participations, notify |
| `RoleAssignedEvent` | MemberId, Role | `Member.AssignRole()` | — |
| `RoleRemovedEvent` | MemberId, Role | `Member.RemoveRole()` | — |

#### Training Events

| Event | Payload | Raised By | Typical Handlers |
|-------|---------|-----------|------------------|
| `TrainingCreatedEvent` | TrainingId | `Training.Create()` | — |
| `TrainingPublishedEvent` | TrainingId, RoomRequirements | `Training.Publish()` | Book rooms |
| `TrainingCanceledEvent` | TrainingId, Reason | `Training.Cancel()` | Cancel bookings, notify |
| `TrainingCompletedEvent` | TrainingId | `Training.Complete()` | — |
| `ParticipantJoinedEvent` | TrainingOrSessionId, MemberId, Status | `AddParticipant()` | Notification |
| `ParticipantCanceledEvent` | TrainingOrSessionId, MemberId | `RemoveParticipant()` | — |
| `ParticipantPromotedFromWaitlistEvent` | TrainingOrSessionId, MemberId | auto-promotion | Notification |
| `AttendanceRecordedEvent` | TrainingOrSessionId, MemberId, Attended | `RecordAttendance()` | — |
| `RecurringTrainingCreatedEvent` | RecurringTrainingId | `RecurringTraining.Create()` | Generate initial sessions |
| `RecurringTrainingPausedEvent` | RecurringTrainingId | `Pause()` | Stop generation |
| `RecurringTrainingEndedEvent` | RecurringTrainingId | `End()` | — |
| `RecurringTrainingTemplateUpdatedEvent` | RecurringTrainingId, Template | `UpdateTemplate()` | Update future sessions |
| `SessionsRequestedEvent` | RecurringTrainingId, Template, Dates, Rule | `GenerateSessionsUntil()` | Create sessions + book rooms |
| `TrainingSessionCanceledEvent` | SessionId, RecurringTrainingId, Reason | `Cancel()` | Cancel bookings |

#### Facility Events

| Event | Payload | Raised By | Typical Handlers |
|-------|---------|-----------|------------------|
| `BookingCreatedEvent` | BookingId, RoomId, LocationId, TimeSlot | `Booking.Create()` | — |
| `BookingCanceledEvent` | BookingId | `Booking.Cancel()` | — |
| `RoomDisabledEvent` | LocationId, RoomId | `Location.DisableRoom()` | Warn affected trainings |

### Event Handling Categories

**Synchronous (in-request, critical path):**
- `TrainingPublishedEvent` → create room bookings (must succeed before response)
- `ParticipantCanceledEvent` → waitlist promotion (must be atomic)

**Asynchronous (background, eventual consistency):**
- `MemberSuspendedEvent` → cancel all active participations
- `RecurringTrainingTemplateUpdatedEvent` → update future sessions
- All notification dispatches

---

## 6. API Design

### CQRS with MediatR

The application layer uses CQRS: commands mutate state, queries read state. Both are dispatched through MediatR.

```
Controller/Endpoint
    │
    ├── Command → IRequestHandler<TCommand, TResult>
    │                   │
    │                   ├── Load aggregate from repository
    │                   ├── Call aggregate method
    │                   ├── Save aggregate
    │                   └── Dispatch domain events
    │
    └── Query → IRequestHandler<TQuery, TResult>
                        │
                        └── Read from MongoDB (direct or read model)
```

### API Resource Design

Base URL: `/api/v1`

#### Membership

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | `/members/register` | Register new member | Public |
| GET | `/members/me` | Get own profile | Member+ |
| PUT | `/members/me` | Update own profile | Member+ |
| GET | `/members` | List members (paginated, filterable) | Admin |
| GET | `/members/{id}` | Get member details | Admin |
| POST | `/members/{id}/approve` | Approve registration | Admin |
| POST | `/members/{id}/reject` | Reject registration | Admin |
| POST | `/members/{id}/suspend` | Suspend member | Admin |
| POST | `/members/{id}/reinstate` | Reinstate member | Admin |
| POST | `/members/{id}/roles` | Assign role | Admin |
| DELETE | `/members/{id}/roles/{role}` | Remove role | Admin |
| GET | `/members/pending` | List pending registrations | Admin |

#### Trainings

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | `/trainings` | Create training | Trainer+ |
| GET | `/trainings` | List trainings (paginated, filterable) | Member+ |
| GET | `/trainings/{id}` | Get training details | Member+ |
| PUT | `/trainings/{id}` | Update training (draft only) | Trainer+ |
| POST | `/trainings/{id}/publish` | Publish training | Trainer+ |
| POST | `/trainings/{id}/cancel` | Cancel training | Trainer+ |
| POST | `/trainings/{id}/complete` | Mark completed | Trainer+ |
| POST | `/trainings/{id}/participants` | Join training | Member+ |
| DELETE | `/trainings/{id}/participants/me` | Leave training | Member+ |
| POST | `/trainings/{id}/attendance` | Record attendance (batch) | Trainer+ |
| POST | `/trainings/{id}/trainers` | Assign trainer | Admin |
| DELETE | `/trainings/{id}/trainers/{trainerId}` | Remove trainer | Admin |

#### Recurring Trainings

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | `/recurring-trainings` | Create recurring training | Trainer+ |
| GET | `/recurring-trainings` | List recurring trainings | Member+ |
| GET | `/recurring-trainings/{id}` | Get details + upcoming sessions | Member+ |
| PUT | `/recurring-trainings/{id}` | Update template | Trainer+ |
| POST | `/recurring-trainings/{id}/pause` | Pause generation | Trainer+ |
| POST | `/recurring-trainings/{id}/resume` | Resume generation | Trainer+ |
| POST | `/recurring-trainings/{id}/end` | End recurring training | Trainer+ |

#### Training Sessions

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/sessions` | List sessions (filterable by date range, recurring ID) | Member+ |
| GET | `/sessions/{id}` | Get session details | Member+ |
| PUT | `/sessions/{id}/overrides` | Apply overrides to session | Trainer+ |
| POST | `/sessions/{id}/reset` | Reset to template | Trainer+ |
| POST | `/sessions/{id}/cancel` | Cancel single session | Trainer+ |
| POST | `/sessions/{id}/complete` | Mark completed | Trainer+ |
| POST | `/sessions/{id}/participants` | Join session | Member+ |
| DELETE | `/sessions/{id}/participants/me` | Leave session | Member+ |
| POST | `/sessions/{id}/attendance` | Record attendance | Trainer+ |

#### Locations & Rooms

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | `/locations` | Create location | Admin |
| GET | `/locations` | List locations | Member+ |
| GET | `/locations/{id}` | Get location with rooms | Member+ |
| PUT | `/locations/{id}` | Update location | Admin |
| POST | `/locations/{id}/rooms` | Add room | Admin |
| PUT | `/locations/{id}/rooms/{roomId}` | Update room | Admin |
| POST | `/locations/{id}/rooms/{roomId}/disable` | Disable room | Admin |
| POST | `/locations/{id}/rooms/{roomId}/enable` | Enable room | Admin |

#### Bookings

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | `/bookings` | Create manual booking | Admin |
| GET | `/bookings` | List bookings (filterable) | Member+ |
| POST | `/bookings/{id}/cancel` | Cancel booking | Admin |
| POST | `/bookings/{id}/reschedule` | Reschedule booking | Admin |
| GET | `/rooms/{roomId}/availability` | Check room availability (date range) | Member+ |

#### Schedule & Calendar

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/schedule/me` | Personal schedule (trainings + sessions) | Member+ |
| GET | `/schedule/trainers/{id}` | Trainer schedule | Trainer+ |
| GET | `/schedule/rooms/{id}` | Room calendar | Member+ |
| GET | `/schedule/export` | Export ICS calendar | Member+ |

#### Reporting

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/reports/room-utilization` | Room utilization stats | Admin |
| GET | `/reports/participation` | Participation statistics | Admin |
| GET | `/reports/trainer-workload` | Trainer workload | Admin |
| GET | `/reports/attendance` | Attendance statistics | Admin |
| GET | `/reports/membership` | Membership statistics | Admin |

### API Conventions

- **Pagination:** `?page=1&pageSize=20` on all list endpoints. Response includes `totalCount`, `page`, `pageSize`, `items[]`.
- **Filtering:** Query parameters specific to each resource (e.g., `?status=Published&from=2026-04-01&to=2026-04-30`).
- **Sorting:** `?sortBy=startDate&sortDirection=desc`
- **Error responses:** Problem Details (RFC 9457) with `type`, `title`, `status`, `detail`, `errors[]`.
- **Versioning:** URL path versioning (`/api/v1/...`).
- **Authentication:** Bearer token (JWT from Keycloak) in `Authorization` header.
- **Date/time:** ISO 8601 with timezone offset (`DateTimeOffset`).
- **IDs:** GUIDs in URL paths and request/response bodies.

### Request/Response DTOs

Commands and queries are defined in the Application layer. Endpoint DTOs (request/response models) are defined in the Api layer and mapped to/from commands/queries.

Example flow for joining a training:

```csharp
// Api layer - endpoint
app.MapPost("/api/v1/trainings/{id}/participants", async (
    Guid id, ISender mediator, ClaimsPrincipal user) =>
{
    var memberId = user.GetMemberId();
    var command = new JoinTrainingCommand(TrainingId.From(id), memberId);
    var result = await mediator.Send(command);
    return result.Match(
        success => Results.Ok(),
        error => Results.Problem(error.ToProblemDetails()));
});

// Application layer - command handler
public class JoinTrainingCommandHandler : IRequestHandler<JoinTrainingCommand, Result>
{
    public async Task<Result> Handle(JoinTrainingCommand request, CancellationToken ct)
    {
        var training = await _trainingRepository.GetByIdAsync(request.TrainingId, ct);
        training.AddParticipant(request.MemberId);
        await _trainingRepository.SaveAsync(training, ct);
        return Result.Success();
    }
}
```

---

## 7. MongoDB Schema Strategy

### Collection-Per-Aggregate

Each aggregate root maps to one MongoDB collection. This is the natural fit for DDD + MongoDB because:
- Aggregates are consistency boundaries → documents are atomically updated
- Embedded entities (Participant, Room) live inside the parent document
- No joins needed within an aggregate

### Collections

| Collection | Aggregate | Typical Document Size | Growth Pattern |
|------------|-----------|----------------------|----------------|
| `members` | Member | ~500 bytes | Linear with user count |
| `trainings` | Training | 2-10 KB (participants embedded) | Grows with participant list |
| `recurring_trainings` | RecurringTraining | ~1 KB | Small, stable |
| `training_sessions` | TrainingSession | 2-10 KB | High volume, time-series |
| `locations` | Location | 1-5 KB (rooms embedded) | Small, stable |
| `bookings` | Booking | ~500 bytes | High volume, time-series |

### Additional Collections (Application Layer)

| Collection | Purpose |
|------------|---------|
| `notifications` | Notification delivery tracking |
| `audit_log` | User action audit trail |
| `outbox` | Transactional outbox for reliable event delivery |

### Index Strategy

```javascript
// members
db.members.createIndex({ "email.value": 1 }, { unique: true })
db.members.createIndex({ "externalIdentity.subjectId": 1 }, { unique: true })
db.members.createIndex({ "registrationStatus": 1 })

// trainings
db.trainings.createIndex({ "status": 1, "timeSlot.start": 1 })
db.trainings.createIndex({ "trainerIds": 1 })
db.trainings.createIndex({ "participants.memberId": 1 })
db.trainings.createIndex({ "timeSlot.start": 1, "timeSlot.end": 1 })

// training_sessions
db.training_sessions.createIndex({ "recurringTrainingId": 1, "timeSlot.start": 1 })
db.training_sessions.createIndex({ "status": 1, "timeSlot.start": 1 })
db.training_sessions.createIndex({ "participants.memberId": 1 })
db.training_sessions.createIndex({ "effectiveTrainerIds": 1, "timeSlot.start": 1 })

// recurring_trainings
db.recurring_trainings.createIndex({ "status": 1 })
db.recurring_trainings.createIndex({ "template.trainerIds": 1 })

// locations
db.locations.createIndex({ "name.value": 1 }, { unique: true })
db.locations.createIndex({ "rooms.roomId": 1 })

// bookings — critical for double-booking prevention
db.bookings.createIndex(
    { "roomId": 1, "timeSlot.start": 1, "timeSlot.end": 1 },
    { partialFilterExpression: { "status": "Active" } }
)
db.bookings.createIndex({ "reference.referenceType": 1, "reference.referenceId": 1 })
db.bookings.createIndex({ "locationId": 1, "status": 1, "timeSlot.start": 1 })

// outbox
db.outbox.createIndex({ "processedAt": 1 }, { expireAfterSeconds: 604800 }) // 7 day TTL
db.outbox.createIndex({ "processedAt": 1, "createdAt": 1 })

// audit_log
db.audit_log.createIndex({ "timestamp": 1 }, { expireAfterSeconds: 7776000 }) // 90 day TTL
db.audit_log.createIndex({ "userId": 1, "timestamp": -1 })
db.audit_log.createIndex({ "entityType": 1, "entityId": 1 })
```

### Optimistic Concurrency

Every aggregate document includes a `version` field (managed by `AggregateRoot<TId>.Version`). The repository uses `findOneAndUpdate` with `{ _id: id, version: expectedVersion }` as the filter. If no document matches (concurrent modification), it throws a concurrency exception.

```csharp
var filter = Builders<TrainingDocument>.Filter.And(
    Builders<TrainingDocument>.Filter.Eq(d => d.Id, id),
    Builders<TrainingDocument>.Filter.Eq(d => d.Version, expectedVersion)
);
var result = await collection.FindOneAndReplaceAsync(filter, document);
if (result == null) throw new ConcurrencyException(typeof(Training), id);
```

### Double-Booking Prevention

Two-layer defense:
1. **Application layer:** `IRoomBookingService.HasConflictAsync()` queries active bookings for time overlap before creating a new booking.
2. **Database layer:** MongoDB Change Stream or a unique constraint approach — since MongoDB doesn't natively support range-overlap unique indexes, we use optimistic concurrency + re-check on conflict.

The overlap check query:

```javascript
db.bookings.find({
    "roomId": targetRoomId,
    "status": "Active",
    "timeSlot.start": { $lt: newEnd },
    "timeSlot.end": { $gt: newStart }
})
```

### Transactional Outbox Pattern

For reliable domain event delivery (e.g., training canceled → bookings canceled), use the transactional outbox:

1. When saving an aggregate, also insert its domain events into the `outbox` collection within the same MongoDB session/transaction.
2. A background worker polls the outbox for unprocessed events and dispatches them.
3. After successful dispatch, marks the outbox entry as processed.

This guarantees at-least-once delivery even if the process crashes between saving the aggregate and publishing events.

---

## 8. Clean Architecture Project Structure

### Solution Layout

```
TrainingOrganizer.slnx
│
├── src/
│   ├── TrainingOrganizer.Domain/               ← INNERMOST LAYER (no deps)
│   │   ├── Common/
│   │   │   ├── Entity.cs
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── ValueObject.cs
│   │   │   ├── StronglyTypedId.cs
│   │   │   ├── IDomainEvent.cs
│   │   │   ├── Guard.cs
│   │   │   └── ValueObjects/
│   │   │       └── TimeSlot.cs
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs
│   │   │   ├── BusinessRuleViolationException.cs
│   │   │   ├── EntityNotFoundException.cs
│   │   │   └── InvalidEntityStateException.cs
│   │   ├── Membership/
│   │   │   ├── Member.cs                       ← Aggregate root
│   │   │   ├── ValueObjects/                   ← MemberId, PersonName, Email, etc.
│   │   │   ├── Enums/                          ← MemberRole, RegistrationStatus
│   │   │   └── Events/                         ← MemberRegistered, etc.
│   │   ├── Training/
│   │   │   ├── Training.cs                     ← Aggregate root
│   │   │   ├── RecurringTraining.cs            ← Aggregate root
│   │   │   ├── TrainingSession.cs              ← Aggregate root
│   │   │   ├── Entities/
│   │   │   │   └── Participant.cs
│   │   │   ├── ParticipantManager.cs           ← Internal helper
│   │   │   ├── ValueObjects/                   ← TrainingId, Capacity, etc.
│   │   │   ├── Enums/                          ← TrainingStatus, Visibility, etc.
│   │   │   └── Events/                         ← TrainingCreated, etc.
│   │   ├── Facility/
│   │   │   ├── Location.cs                     ← Aggregate root
│   │   │   ├── Booking.cs                      ← Aggregate root
│   │   │   ├── Entities/
│   │   │   │   └── Room.cs
│   │   │   ├── ValueObjects/                   ← LocationId, RoomId, Address, etc.
│   │   │   ├── Enums/                          ← RoomStatus, BookingStatus, etc.
│   │   │   └── Events/                         ← BookingCreated, etc.
│   │   └── Services/                           ← Domain service INTERFACES
│   │       ├── IRoomBookingService.cs
│   │       ├── ISessionGenerationService.cs
│   │       └── IMemberUniquenessService.cs
│   │
│   ├── TrainingOrganizer.Application/          ← USE CASES (refs Domain)
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IUnitOfWork.cs
│   │   │   │   ├── ICurrentUserService.cs
│   │   │   │   └── IDateTimeProvider.cs
│   │   │   ├── Behaviors/                      ← MediatR pipeline behaviors
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── TransactionBehavior.cs
│   │   │   ├── Exceptions/
│   │   │   │   ├── ValidationException.cs
│   │   │   │   └── ForbiddenException.cs
│   │   │   ├── Models/
│   │   │   │   ├── Result.cs                   ← Result<T> monad
│   │   │   │   └── PagedList.cs
│   │   │   └── Mappings/
│   │   │       └── MappingExtensions.cs
│   │   ├── Membership/
│   │   │   ├── Repositories/
│   │   │   │   └── IMemberRepository.cs
│   │   │   ├── Commands/
│   │   │   │   ├── RegisterMember/
│   │   │   │   │   ├── RegisterMemberCommand.cs
│   │   │   │   │   ├── RegisterMemberCommandHandler.cs
│   │   │   │   │   └── RegisterMemberCommandValidator.cs
│   │   │   │   ├── ApproveMember/
│   │   │   │   ├── RejectMember/
│   │   │   │   ├── SuspendMember/
│   │   │   │   ├── ReinstateMember/
│   │   │   │   ├── AssignRole/
│   │   │   │   └── UpdateProfile/
│   │   │   ├── Queries/
│   │   │   │   ├── GetMember/
│   │   │   │   ├── GetCurrentMember/
│   │   │   │   ├── ListMembers/
│   │   │   │   └── ListPendingMembers/
│   │   │   └── EventHandlers/
│   │   │       └── MemberSuspendedHandler.cs   ← cancel participations
│   │   ├── Training/
│   │   │   ├── Repositories/
│   │   │   │   ├── ITrainingRepository.cs
│   │   │   │   ├── IRecurringTrainingRepository.cs
│   │   │   │   └── ITrainingSessionRepository.cs
│   │   │   ├── Commands/
│   │   │   │   ├── CreateTraining/
│   │   │   │   ├── PublishTraining/
│   │   │   │   ├── CancelTraining/
│   │   │   │   ├── JoinTraining/
│   │   │   │   ├── LeaveTraining/
│   │   │   │   ├── RecordAttendance/
│   │   │   │   ├── CreateRecurringTraining/
│   │   │   │   ├── PauseRecurringTraining/
│   │   │   │   ├── CancelSession/
│   │   │   │   └── ...
│   │   │   ├── Queries/
│   │   │   │   ├── GetTraining/
│   │   │   │   ├── ListTrainings/
│   │   │   │   ├── ListSessions/
│   │   │   │   └── GetPersonalSchedule/
│   │   │   └── EventHandlers/
│   │   │       ├── TrainingPublishedHandler.cs  ← book rooms
│   │   │       ├── TrainingCanceledHandler.cs   ← cancel bookings
│   │   │       └── SessionsRequestedHandler.cs  ← generate sessions
│   │   ├── Facility/
│   │   │   ├── Repositories/
│   │   │   │   ├── ILocationRepository.cs
│   │   │   │   └── IBookingRepository.cs
│   │   │   ├── Commands/
│   │   │   │   ├── CreateLocation/
│   │   │   │   ├── AddRoom/
│   │   │   │   ├── CreateBooking/
│   │   │   │   ├── CancelBooking/
│   │   │   │   └── ...
│   │   │   ├── Queries/
│   │   │   │   ├── GetLocation/
│   │   │   │   ├── ListLocations/
│   │   │   │   ├── GetRoomAvailability/
│   │   │   │   └── GetRoomCalendar/
│   │   │   └── EventHandlers/
│   │   │       └── RoomDisabledHandler.cs
│   │   ├── Notifications/
│   │   │   ├── INotificationService.cs
│   │   │   ├── NotificationTemplates.cs
│   │   │   └── EventHandlers/
│   │   │       ├── SendParticipantJoinedNotification.cs
│   │   │       ├── SendTrainingCanceledNotification.cs
│   │   │       └── ...
│   │   ├── Scheduling/
│   │   │   ├── ISchedulingService.cs
│   │   │   └── Jobs/
│   │   │       ├── GenerateSessionsJob.cs
│   │   │       ├── SendRemindersJob.cs
│   │   │       └── AutoCancelEmptyTrainingsJob.cs
│   │   └── Reporting/
│   │       └── Queries/
│   │           ├── GetRoomUtilization/
│   │           ├── GetParticipationStats/
│   │           ├── GetTrainerWorkload/
│   │           └── GetAttendanceStats/
│   │
│   ├── TrainingOrganizer.Infrastructure/       ← EXTERNAL CONCERNS (refs Application)
│   │   ├── Persistence/
│   │   │   ├── MongoDbContext.cs
│   │   │   ├── MongoDbSettings.cs
│   │   │   ├── UnitOfWork.cs                   ← implements IUnitOfWork
│   │   │   ├── Serialization/
│   │   │   │   ├── BsonClassMapRegistrations.cs
│   │   │   │   └── StronglyTypedIdSerializer.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── MemberRepository.cs
│   │   │   │   ├── TrainingRepository.cs
│   │   │   │   ├── RecurringTrainingRepository.cs
│   │   │   │   ├── TrainingSessionRepository.cs
│   │   │   │   ├── LocationRepository.cs
│   │   │   │   └── BookingRepository.cs
│   │   │   └── Migrations/
│   │   │       └── IndexMigrationRunner.cs     ← creates indexes on startup
│   │   ├── Services/
│   │   │   ├── RoomBookingService.cs           ← implements IRoomBookingService
│   │   │   ├── SessionGenerationService.cs     ← implements ISessionGenerationService
│   │   │   ├── MemberUniquenessService.cs      ← implements IMemberUniquenessService
│   │   │   ├── DateTimeProvider.cs             ← implements IDateTimeProvider
│   │   │   └── CurrentUserService.cs           ← implements ICurrentUserService
│   │   ├── Identity/
│   │   │   ├── KeycloakConfiguration.cs
│   │   │   ├── KeycloakTokenValidation.cs
│   │   │   └── ClaimsPrincipalExtensions.cs
│   │   ├── Notifications/
│   │   │   ├── EmailNotificationService.cs
│   │   │   ├── InAppNotificationService.cs
│   │   │   └── Templates/                      ← email templates
│   │   ├── BackgroundJobs/
│   │   │   ├── SessionGenerationWorker.cs      ← IHostedService
│   │   │   ├── ReminderWorker.cs
│   │   │   └── OutboxProcessorWorker.cs        ← dispatches outbox events
│   │   ├── Outbox/
│   │   │   ├── OutboxMessage.cs
│   │   │   ├── OutboxRepository.cs
│   │   │   └── OutboxProcessor.cs
│   │   └── DependencyInjection.cs              ← all Infrastructure DI registrations
│   │
│   └── TrainingOrganizer.Api/                  ← OUTERMOST LAYER (refs Infrastructure)
│       ├── Program.cs                          ← composition root
│       ├── Endpoints/
│       │   ├── MemberEndpoints.cs
│       │   ├── TrainingEndpoints.cs
│       │   ├── RecurringTrainingEndpoints.cs
│       │   ├── SessionEndpoints.cs
│       │   ├── LocationEndpoints.cs
│       │   ├── BookingEndpoints.cs
│       │   ├── ScheduleEndpoints.cs
│       │   └── ReportEndpoints.cs
│       ├── Contracts/                          ← Request/Response DTOs
│       │   ├── Membership/
│       │   ├── Training/
│       │   ├── Facility/
│       │   └── Common/
│       │       ├── PagedResponse.cs
│       │       └── ProblemDetailsFactory.cs
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   ├── RequestLoggingMiddleware.cs
│       │   └── CorrelationIdMiddleware.cs
│       ├── Filters/
│       │   └── ValidationFilter.cs
│       └── appsettings.json / appsettings.*.json
│
├── tests/
│   ├── TrainingOrganizer.Domain.Tests/
│   │   ├── Membership/
│   │   │   └── MemberTests.cs
│   │   ├── Training/
│   │   │   ├── TrainingTests.cs
│   │   │   ├── RecurringTrainingTests.cs
│   │   │   ├── TrainingSessionTests.cs
│   │   │   └── ParticipantManagerTests.cs
│   │   ├── Facility/
│   │   │   ├── LocationTests.cs
│   │   │   └── BookingTests.cs
│   │   └── Common/
│   │       └── TimeSlotTests.cs
│   │
│   ├── TrainingOrganizer.Application.Tests/
│   │   ├── Membership/
│   │   │   └── Commands/
│   │   │       └── RegisterMemberCommandHandlerTests.cs
│   │   └── Training/
│   │       └── Commands/
│   │           └── JoinTrainingCommandHandlerTests.cs
│   │
│   └── TrainingOrganizer.Integration.Tests/    ← NEW: integration tests
│       ├── Infrastructure/
│       │   ├── MongoDbFixture.cs               ← testcontainers for MongoDB
│       │   └── Repositories/
│       │       └── MemberRepositoryTests.cs
│       └── Api/
│           ├── WebApplicationFixture.cs
│           └── Endpoints/
│               └── TrainingEndpointTests.cs
│
├── docker/
│   ├── Dockerfile
│   ├── docker-compose.yml                      ← local dev (API + MongoDB + Keycloak)
│   └── docker-compose.override.yml
│
├── k8s/                                        ← Kubernetes manifests
│   └── (see section 10)
│
├── docs/
│   ├── domain-model.md
│   └── architecture-blueprint.md               ← this document
│
└── CLAUDE.md
```

### Dependency Rules (Strictly Enforced)

```
Domain        → (nothing)
Application   → Domain
Infrastructure→ Application (transitively Domain)
Api           → Infrastructure (transitively Application, Domain)
Tests         → Project under test + test dependencies
```

The Domain project has **zero NuGet dependencies**. The Application project depends on MediatR and FluentValidation. Infrastructure depends on MongoDB driver, Keycloak SDK, email libraries. Api depends on ASP.NET Core and Swagger.

### NuGet Packages by Layer

**Application:**
- `MediatR` — CQRS dispatch
- `FluentValidation` — command/query validation
- `FluentValidation.DependencyInjectionExtensions`

**Infrastructure:**
- `MongoDB.Driver` — persistence
- `Microsoft.AspNetCore.Authentication.JwtBearer` — Keycloak JWT validation
- `Microsoft.Extensions.Hosting` — background workers
- `OpenTelemetry.*` — observability

**Api:**
- `Microsoft.AspNetCore.OpenApi` / `Swashbuckle.AspNetCore` — API documentation
- `Serilog.AspNetCore` — structured logging

**Tests:**
- `xUnit`, `Microsoft.NET.Test.Sdk`
- `NSubstitute` or `Moq` — mocking
- `FluentAssertions` — assertion library
- `Testcontainers.MongoDb` — integration test containers
- `Microsoft.AspNetCore.Mvc.Testing` — API integration tests

---

## 9. Frontend Architecture

### Recommendation: React + React Native (with shared logic)

| Criterion | React + RN | Flutter | Angular | Next.js |
|-----------|-----------|---------|---------|---------|
| Web quality | Excellent | Subpar (Flutter web is beta-grade for business apps) | Excellent | Excellent |
| Mobile path | React Native (shared mental model) | Excellent native | Ionic/Capacitor (mediocre) | Not mobile |
| Code sharing (web ↔ mobile) | ~60-70% shared business logic | 95%+ (single codebase) | None | None |
| Ecosystem / hiring | Largest | Growing but smaller | Mature but shrinking | Large |
| Component libraries | MUI, Ant Design, Radix | Material, limited alternatives | Angular Material | Same as React |
| Calendar components | FullCalendar, react-big-calendar | Limited | FullCalendar for Angular | Same as React |
| Learning curve | Moderate | Moderate (Dart) | Steep | Moderate |
| API-first fit | Excellent | Excellent | Excellent | Excellent |

**Why React + React Native over Flutter:**

1. **Calendar-heavy UI** — The scheduling/calendar views are central to this application. React has mature, battle-tested calendar libraries (FullCalendar, react-big-calendar). Flutter's calendar ecosystem is significantly weaker for complex scheduling UIs.

2. **Web-first priority** — Flutter web still lags behind for data-heavy business applications. React is the production standard.

3. **Incremental mobile adoption** — React Native lets you build mobile apps incrementally, sharing navigation patterns and API client code with the web app. The mental model transfer from React to React Native is nearly seamless.

4. **Ecosystem depth** — Component libraries for admin panels, form builders, data tables, and charts are vastly more mature in React.

**Why not Angular:** While Angular is technically excellent, the team/developer ecosystem is contracting. React's market share advantage means easier hiring and more community support.

**Why not Next.js standalone:** Next.js is a React framework — you'd use it *within* the React choice, not instead of it. However, for this project, a plain React SPA (Vite) is more appropriate since the backend is a separate ASP.NET API. Next.js adds SSR complexity that isn't needed when the backend is already separate.

### Frontend Architecture

```
frontend/
├── packages/
│   ├── web/                    ← React SPA (Vite)
│   │   ├── src/
│   │   │   ├── app/            ← routing, layout, providers
│   │   │   ├── features/       ← feature-based modules
│   │   │   │   ├── auth/
│   │   │   │   ├── members/
│   │   │   │   ├── trainings/
│   │   │   │   ├── schedule/
│   │   │   │   ├── locations/
│   │   │   │   ├── bookings/
│   │   │   │   └── reports/
│   │   │   ├── components/     ← shared UI components
│   │   │   └── hooks/          ← shared hooks
│   │   └── package.json
│   │
│   ├── mobile/                 ← React Native (Expo)
│   │   ├── src/
│   │   │   ├── screens/
│   │   │   ├── navigation/
│   │   │   └── components/
│   │   └── package.json
│   │
│   └── shared/                 ← Shared business logic
│       ├── src/
│       │   ├── api/            ← Generated API client (OpenAPI)
│       │   ├── models/         ← TypeScript types from API
│       │   ├── stores/         ← State management (Zustand/TanStack Query)
│       │   └── utils/          ← Date formatting, validation, etc.
│       └── package.json
│
├── pnpm-workspace.yaml         ← Monorepo with pnpm workspaces
└── package.json
```

### Key Frontend Technologies

| Concern | Choice | Reason |
|---------|--------|--------|
| Build tool | Vite | Fast, modern, excellent DX |
| State / data fetching | TanStack Query (React Query) | Cache, optimistic updates, background refetch |
| Forms | React Hook Form + Zod | Performant forms with type-safe validation |
| UI components | Radix UI + Tailwind CSS | Accessible primitives, utility-first styling |
| Calendar | FullCalendar React | Most feature-complete scheduling component |
| Auth | oidc-client-ts | Standard OIDC/OAuth2 library for Keycloak |
| API client | Auto-generated from OpenAPI spec | Type safety, always in sync with backend |
| Routing | React Router v7 | Standard, widely used |
| Monorepo | pnpm workspaces | Fast, disk-efficient |

### Authentication Flow (Frontend ↔ Keycloak ↔ API)

```
Browser                   Keycloak                 API
   │                         │                      │
   ├── OIDC authorize ──────►│                      │
   │◄── redirect + code ─────│                      │
   ├── token exchange ──────►│                      │
   │◄── access_token + ──────│                      │
   │    refresh_token         │                      │
   │                         │                      │
   ├── API call + Bearer ────┼─────────────────────►│
   │                         │   validate JWT ◄─────│
   │◄────────────────────────┼──────── response ────│
```

---

## 10. Kubernetes Deployment Architecture

### Cluster Topology

```
┌─────────────────────────────────────────────────────────────┐
│                    Kubernetes Cluster                         │
│                                                              │
│  ┌──────────────── training-organizer namespace ──────────┐  │
│  │                                                        │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌──────────────┐  │  │
│  │  │  API        │  │  API        │  │  Background  │  │  │
│  │  │  Pod (1)    │  │  Pod (2)    │  │  Worker Pod  │  │  │
│  │  │  replica    │  │  replica    │  │  (1 replica) │  │  │
│  │  └──────┬──────┘  └──────┬──────┘  └──────┬───────┘  │  │
│  │         │                │                │           │  │
│  │         └────────┬───────┘                │           │  │
│  │                  │                        │           │  │
│  │          ┌───────▼───────┐                │           │  │
│  │          │  Service      │                │           │  │
│  │          │  (ClusterIP)  │                │           │  │
│  │          └───────┬───────┘                │           │  │
│  │                  │                        │           │  │
│  │          ┌───────▼───────┐                │           │  │
│  │          │  Ingress      │                │           │  │
│  │          │  (nginx)      │                │           │  │
│  │          └───────────────┘                │           │  │
│  │                                           │           │  │
│  └───────────────────────────────────────────┼───────────┘  │
│                                              │               │
│  ┌──────────────── data namespace ───────────┼───────────┐  │
│  │                                           │           │  │
│  │  ┌─────────────────┐  ┌──────────────┐   │           │  │
│  │  │  MongoDB         │  │  Keycloak    │   │           │  │
│  │  │  (StatefulSet    │  │  (Deployment │   │           │  │
│  │  │   or Atlas)      │  │   + PG)      │   │           │  │
│  │  └─────────────────┘  └──────────────┘   │           │  │
│  │                                           │           │  │
│  └───────────────────────────────────────────┘           │  │
│                                                              │
│  ┌──────────────── observability namespace ───────────────┐  │
│  │                                                        │  │
│  │  ┌──────────┐  ┌────────────┐  ┌───────┐  ┌────────┐ │  │
│  │  │Prometheus│  │  Grafana   │  │ Loki  │  │ Tempo  │ │  │
│  │  └──────────┘  └────────────┘  └───────┘  └────────┘ │  │
│  │                                                        │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### Kubernetes Manifests (Helm Chart)

```
k8s/
├── charts/
│   └── training-organizer/
│       ├── Chart.yaml
│       ├── values.yaml
│       ├── values-dev.yaml
│       ├── values-staging.yaml
│       ├── values-production.yaml
│       └── templates/
│           ├── _helpers.tpl
│           ├── namespace.yaml
│           ├── api-deployment.yaml
│           ├── api-service.yaml
│           ├── api-hpa.yaml               ← Horizontal Pod Autoscaler
│           ├── api-ingress.yaml
│           ├── worker-deployment.yaml
│           ├── configmap.yaml
│           ├── secret.yaml                ← sealed secrets or external-secrets
│           ├── mongodb-statefulset.yaml   ← (or use MongoDB Atlas operator)
│           ├── keycloak-deployment.yaml
│           ├── networkpolicy.yaml
│           └── serviceaccount.yaml
```

### Key Kubernetes Resources

**API Deployment:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: training-organizer-api
spec:
  replicas: 2
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  template:
    spec:
      containers:
      - name: api
        image: training-organizer-api:latest
        ports:
        - containerPort: 8080
        resources:
          requests:
            cpu: 250m
            memory: 256Mi
          limits:
            cpu: 1000m
            memory: 512Mi
        livenessProbe:
          httpGet:
            path: /healthz
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 15
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
        env:
        - name: MongoDB__ConnectionString
          valueFrom:
            secretKeyRef:
              name: training-organizer-secrets
              key: mongodb-connection-string
        - name: Keycloak__Authority
          valueFrom:
            configMapKeyRef:
              name: training-organizer-config
              key: keycloak-authority
```

**HPA (Horizontal Pod Autoscaler):**
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: training-organizer-api
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: training-organizer-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

**Background Worker (separate deployment, single replica with leader election):**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: training-organizer-worker
spec:
  replicas: 1
  template:
    spec:
      containers:
      - name: worker
        image: training-organizer-api:latest  # same image, different entrypoint
        command: ["dotnet", "TrainingOrganizer.Api.dll", "--worker"]
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 256Mi
```

### Database Strategy

**Development/Staging:** MongoDB as a StatefulSet with a single replica.

**Production:** MongoDB Atlas (managed) or self-hosted replica set (3 nodes). Benefits:
- Automated backups
- Point-in-time recovery
- Monitoring and alerts
- No operational burden of managing stateful sets

### Secrets Management

Use `sealed-secrets` (Bitnami) or `external-secrets-operator` (syncs from Azure Key Vault / AWS Secrets Manager / HashiCorp Vault):

```yaml
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: training-organizer-secrets
spec:
  refreshInterval: 1h
  secretStoreRef:
    name: azure-keyvault
    kind: ClusterSecretStore
  target:
    name: training-organizer-secrets
  data:
  - secretKey: mongodb-connection-string
    remoteRef:
      key: training-organizer-mongodb-conn
```

---

## 11. CI/CD Pipeline Design

### Recommendation: GitHub Actions

Chosen over GitLab CI because the repository is on GitHub and GitHub Actions has excellent container registry integration and Kubernetes deployment actions.

### Pipeline Overview

```
                    ┌─────────────────┐
                    │   Push / PR     │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │    Build        │
                    │  dotnet build   │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
      ┌──────────────┐ ┌──────────┐ ┌──────────────┐
      │  Unit Tests  │ │  Lint    │ │  Security    │
      │  dotnet test │ │  format  │ │  Scan        │
      └──────┬───────┘ └────┬─────┘ └──────┬───────┘
              │              │              │
              └──────────────┼──────────────┘
                             │
                    ┌────────▼────────┐
                    │ Integration     │
                    │ Tests           │
                    │ (testcontainers)│
                    └────────┬────────┘
                             │
              (merge to main only below)
                             │
                    ┌────────▼────────┐
                    │  Docker Build   │
                    │  & Push to      │
                    │  Container Reg  │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
      ┌──────────────┐ ┌──────────┐ ┌──────────────┐
      │  Deploy Dev  │ │ Deploy   │ │  Deploy Prod │
      │  (auto)      │ │ Staging  │ │  (manual     │
      │              │ │ (auto)   │ │   approval)  │
      └──────────────┘ └──────────┘ └──────────────┘
```

### GitHub Actions Workflows

#### `.github/workflows/ci.yml` (runs on every push and PR)

```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: '10.0.x'
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    - run: dotnet restore
    - run: dotnet build --no-restore --configuration Release
    - run: dotnet format --verify-no-changes

  test-unit:
    needs: build
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    - run: dotnet test tests/TrainingOrganizer.Domain.Tests/ --configuration Release --collect:"XPlat Code Coverage"
    - run: dotnet test tests/TrainingOrganizer.Application.Tests/ --configuration Release --collect:"XPlat Code Coverage"
    - uses: codecov/codecov-action@v4

  test-integration:
    needs: build
    runs-on: ubuntu-latest
    services:
      mongodb:
        image: mongo:7
        ports:
        - 27017:27017
    steps:
    - uses: actions/checkout@v4
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    - run: dotnet test tests/TrainingOrganizer.Integration.Tests/ --configuration Release
      env:
        MongoDB__ConnectionString: mongodb://localhost:27017

  security-scan:
    needs: build
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - run: dotnet restore
    - uses: github/codeql-action/init@v3
      with:
        languages: csharp
    - run: dotnet build --no-restore
    - uses: github/codeql-action/analyze@v3

  docker-build:
    needs: [test-unit, test-integration, security-scan]
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    steps:
    - uses: actions/checkout@v4
    - uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    - uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: |
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:latest

  deploy-staging:
    needs: docker-build
    runs-on: ubuntu-latest
    environment: staging
    steps:
    - uses: actions/checkout@v4
    - uses: azure/k8s-set-context@v4
      with:
        kubeconfig: ${{ secrets.KUBE_CONFIG_STAGING }}
    - run: |
        helm upgrade --install training-organizer k8s/charts/training-organizer \
          -f k8s/charts/training-organizer/values-staging.yaml \
          --set image.tag=${{ github.sha }} \
          --namespace training-organizer \
          --wait --timeout 5m

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://app.trainingorganizer.com
    steps:
    - uses: actions/checkout@v4
    - uses: azure/k8s-set-context@v4
      with:
        kubeconfig: ${{ secrets.KUBE_CONFIG_PRODUCTION }}
    - run: |
        helm upgrade --install training-organizer k8s/charts/training-organizer \
          -f k8s/charts/training-organizer/values-production.yaml \
          --set image.tag=${{ github.sha }} \
          --namespace training-organizer \
          --wait --timeout 5m
```

### Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.slnx .
COPY src/TrainingOrganizer.Domain/*.csproj src/TrainingOrganizer.Domain/
COPY src/TrainingOrganizer.Application/*.csproj src/TrainingOrganizer.Application/
COPY src/TrainingOrganizer.Infrastructure/*.csproj src/TrainingOrganizer.Infrastructure/
COPY src/TrainingOrganizer.Api/*.csproj src/TrainingOrganizer.Api/
RUN dotnet restore src/TrainingOrganizer.Api/TrainingOrganizer.Api.csproj

COPY src/ src/
RUN dotnet publish src/TrainingOrganizer.Api/TrainingOrganizer.Api.csproj \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "TrainingOrganizer.Api.dll"]
```

### Docker Compose (Local Development)

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: docker/Dockerfile
    ports:
      - "5100:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MongoDB__ConnectionString=mongodb://mongodb:27017/training-organizer
      - Keycloak__Authority=http://keycloak:8080/realms/training-organizer
    depends_on:
      - mongodb
      - keycloak

  mongodb:
    image: mongo:7
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db

  keycloak:
    image: quay.io/keycloak/keycloak:24.0
    command: start-dev --import-realm
    ports:
      - "8180:8080"
    environment:
      - KEYCLOAK_ADMIN=admin
      - KEYCLOAK_ADMIN_PASSWORD=admin
    volumes:
      - ./docker/keycloak/realm-export.json:/opt/keycloak/data/import/realm.json

volumes:
  mongodb_data:
```

---

## 12. Observability Strategy

### Three Pillars

```
Application Code
    │
    ├── Logs ──────────► Loki ──────────► Grafana (Log Explorer)
    │   (Serilog → OTLP)
    │
    ├── Metrics ───────► Prometheus ────► Grafana (Dashboards)
    │   (OpenTelemetry)
    │
    └── Traces ────────► Tempo ─────────► Grafana (Trace View)
        (OpenTelemetry)
```

### Implementation

#### Structured Logging (Serilog + OpenTelemetry)

```csharp
// Program.cs
builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "TrainingOrganizer")
    .WriteTo.OpenTelemetry(options =>
    {
        options.Endpoint = builder.Configuration["Otlp:Endpoint"];
        options.Protocol = OtlpProtocol.Grpc;
    }));
```

Log correlation: every request gets a `CorrelationId` (from header or generated), propagated through all log entries and traces.

#### Metrics (OpenTelemetry + Prometheus)

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("TrainingOrganizer")          // custom metrics
        .AddPrometheusExporter());
```

**Custom metrics to track:**

| Metric | Type | Labels | Purpose |
|--------|------|--------|---------|
| `training.participants.joined` | Counter | training_type, status | Participation trends |
| `training.participants.active` | Gauge | training_id | Current occupancy |
| `booking.conflicts.detected` | Counter | room_id | Double-booking prevention health |
| `session.generation.duration` | Histogram | — | Generation performance |
| `session.generation.count` | Counter | recurring_training_id | Sessions created |
| `member.registrations` | Counter | status | Registration flow health |
| `outbox.messages.pending` | Gauge | — | Event delivery backlog |
| `outbox.messages.processed` | Counter | success/failure | Event delivery reliability |

#### Distributed Tracing (OpenTelemetry + Tempo)

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("MongoDB.Driver")            // MongoDB operations
        .AddSource("TrainingOrganizer")          // custom activities
        .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));
```

Traces automatically capture:
- HTTP request → command handler → repository → MongoDB query
- Domain event dispatch → event handler → side effects
- Background worker execution cycles

#### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddMongoDb(connectionString, name: "mongodb")
    .AddUrlGroup(new Uri(keycloakUrl), name: "keycloak");

app.MapHealthChecks("/healthz", new() { Predicate = _ => true });
app.MapHealthChecks("/ready", new() {
    Predicate = check => check.Tags.Contains("ready")
});
```

#### Grafana Dashboards

Pre-built dashboards for:

1. **API Overview** — request rate, error rate, p50/p95/p99 latency, active connections
2. **Training Operations** — trainings created/published/canceled, participation join/leave rates, waitlist depth
3. **Booking Health** — bookings created/canceled, conflict detection rate, room utilization
4. **Background Workers** — outbox lag, session generation timing, job execution success rate
5. **Infrastructure** — MongoDB connection pool, query latency, CPU/memory usage

### Alerting Rules (Prometheus)

```yaml
groups:
- name: training-organizer
  rules:
  - alert: HighErrorRate
    expr: rate(http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m]) > 0.05
    for: 2m
    labels:
      severity: critical

  - alert: OutboxBacklog
    expr: outbox_messages_pending > 100
    for: 5m
    labels:
      severity: warning

  - alert: MongoDBLatencyHigh
    expr: histogram_quantile(0.95, rate(mongodb_command_duration_seconds_bucket[5m])) > 0.5
    for: 3m
    labels:
      severity: warning

  - alert: BookingConflictSpike
    expr: rate(booking_conflicts_detected_total[5m]) > 1
    for: 1m
    labels:
      severity: warning
```

---

## 13. Scalability Considerations

### Scaling Profile

This system has **asymmetric load patterns:**
- **Read-heavy:** Calendar views, schedule queries, availability checks (~90% of traffic)
- **Write-heavy bursts:** Training registration opens (many participants joining simultaneously)
- **Background processing:** Session generation, reminder dispatch (predictable, schedulable)

### Scaling Strategy

#### Tier 1: Vertical (sufficient for 0–5,000 members)

- Single API pod (2-4 CPU, 1-2 GB RAM)
- Single MongoDB instance
- Background worker in same process
- This handles a typical training organization comfortably

#### Tier 2: Horizontal API (5,000–50,000 members)

- 2-5 API pods behind load balancer (HPA on CPU/request rate)
- MongoDB replica set (1 primary + 2 secondaries)
- Read queries routed to secondaries (ReadPreference.SecondaryPreferred for non-critical reads)
- Separate worker deployment (1-2 pods)
- Redis for distributed caching (session data, room availability cache)

#### Tier 3: Read/Write Split (50,000+ members)

- CQRS read models in separate MongoDB collections (or Redis)
- Dedicated query endpoints that bypass the domain layer
- MongoDB sharding by locationId or date range (if single-location datasets get too large)
- Extract notification service as independent deployment
- Consider message broker (RabbitMQ/NATS) for async event delivery

### Specific Bottleneck Analysis

**Participant join during registration opens:**
The `Training` aggregate with embedded participants is the bottleneck. When 100 people try to join simultaneously, optimistic concurrency will cause retries.

*Mitigation:*
1. Retry with exponential backoff in the command handler (3 retries)
2. For very popular trainings, consider a "reservation" pattern: accept joins into a queue, process sequentially
3. The capacity invariant makes this safe — worst case is optimistic concurrency failures, never overbooking

**Calendar/schedule queries:**
These span multiple collections (trainings, sessions, bookings) and can be expensive.

*Mitigation:*
1. Materialized views: maintain denormalized `schedule_entries` collection updated by event handlers
2. Query caching with short TTL (30-60 seconds) for room availability
3. Date-range filtering pushes the heavy lifting to MongoDB indexes

**Session generation:**
`RecurringTraining.GenerateSessionsUntil()` can produce many sessions at once.

*Mitigation:*
1. Rolling window (4 weeks ahead) limits batch size
2. Idempotent via `lastGeneratedUntil` — safe to retry
3. Runs in background worker, doesn't block API requests

### MongoDB Scaling Path

```
Phase 1: Single instance (dev/small prod)
    ↓
Phase 2: Replica set (3 nodes) — read scaling + HA
    ↓
Phase 3: MongoDB Atlas (managed) — auto-scaling, backups, monitoring
    ↓
Phase 4: Sharding by tenant/location (only if multi-tenant or extreme scale)
```

### Caching Strategy

| Data | Cache Location | TTL | Invalidation |
|------|---------------|-----|--------------|
| Room availability | Redis | 30s | On booking create/cancel |
| Member profile (for display) | In-memory (IMemoryCache) | 5min | On profile update |
| Training list (published) | Redis | 60s | On training status change |
| Location/room list | In-memory | 10min | On location update |

---

## 14. Future Mobile Support Strategy

### Phase 1: Mobile-Optimized Web (Week 1-N)

Before building native apps, ensure the React web app is fully responsive. This gives mobile users immediate access while native apps are developed.

Key mobile-web optimizations:
- Responsive Tailwind CSS layouts
- Touch-friendly calendar interactions
- PWA support (installable, offline schedule viewing)
- Push notifications via Web Push API

### Phase 2: React Native App (When Justified)

**When to start:**
- User base exceeds ~1,000 active mobile users
- Push notification reliability on mobile web is insufficient
- Need native device features (camera for attendance QR scan, GPS for check-in)

### Architecture for Code Sharing

```
┌──────────────────────────────────────────┐
│            Shared Package                 │
│                                           │
│  ├── API client (generated from OpenAPI)  │
│  ├── TypeScript types / models            │
│  ├── Business logic (date formatting,     │
│  │   validation, permission checks)       │
│  ├── State management (TanStack Query     │
│  │   configs, cache keys)                 │
│  └── Constants, enums                     │
│                                           │
└───────────┬───────────────┬───────────────┘
            │               │
    ┌───────▼──────┐ ┌──────▼───────┐
    │  React Web   │ │ React Native │
    │              │ │              │
    │  - Routing   │ │  - Navigation│
    │  - Layout    │ │  - Native UI │
    │  - Web UI    │ │  - Push notif│
    │  - Calendar  │ │  - Camera    │
    │    (browser) │ │  - Offline   │
    └──────────────┘ └──────────────┘
```

**Shared (60-70%):**
- API client and data fetching hooks
- Business logic (who can do what, date calculations)
- Form validation schemas (Zod)
- Type definitions

**Platform-specific (30-40%):**
- UI components (React web components vs React Native components)
- Navigation (React Router vs React Navigation)
- Storage (localStorage vs AsyncStorage)
- Push notifications (Web Push vs Firebase/APNs)

### React Native Technology Stack

| Concern | Choice |
|---------|--------|
| Framework | React Native with Expo |
| Navigation | React Navigation v7 |
| State | TanStack Query (same as web) |
| UI | React Native Paper or Tamagui |
| Push | expo-notifications → Firebase/APNs |
| Offline | WatermelonDB or MMKV for local cache |
| Calendar | react-native-calendars |

### Mobile-Specific Features

| Feature | Implementation |
|---------|---------------|
| QR code attendance | Camera scan → `POST /sessions/{id}/attendance` |
| Location check-in | GPS coordinates → validate proximity to location |
| Offline schedule | Cache upcoming schedule locally, sync on reconnect |
| Push notifications | Firebase Cloud Messaging (Android + iOS) |
| Quick join | Deep link from push notification → join training |
| Biometric auth | Expo SecureStore + FaceID/Fingerprint |

### API Considerations for Mobile

The same REST API serves both web and mobile. No separate mobile API needed. Key accommodations:

1. **Pagination everywhere** — mobile has limited memory
2. **Sparse fieldsets** — `?fields=id,title,timeSlot,status` to reduce payload size (implement later if needed)
3. **ETag/If-None-Match** — avoid re-downloading unchanged data
4. **Offline-first patterns** — API returns `Last-Modified` headers; mobile caches and sends `If-Modified-Since`

---

## Appendix: Implementation Priority

### Phase 1 — Foundation (MVP)

| Component | Priority | Status |
|-----------|----------|--------|
| Domain layer | Done | Complete (66 files) |
| Application layer (CQRS + MediatR) | High | Scaffold only |
| MongoDB persistence (repositories) | High | Not started |
| Keycloak integration | High | Not started |
| API endpoints (core CRUD) | High | Scaffold only |
| Unit tests (domain) | High | Not started |
| Docker Compose (local dev) | High | Not started |

### Phase 2 — Core Features

| Component | Priority |
|-----------|----------|
| Participation (join/leave/waitlist) | High |
| Recurring training session generation | High |
| Room booking + conflict checking | High |
| Integration tests | Medium |
| CI/CD pipeline (GitHub Actions) | Medium |
| Basic notification (email) | Medium |

### Phase 3 — Production Readiness

| Component | Priority |
|-----------|----------|
| Observability (logging, metrics, tracing) | High |
| Kubernetes deployment (Helm) | High |
| Security hardening | High |
| Frontend (React SPA) | High |
| Attendance tracking | Medium |
| Calendar export (ICS) | Medium |

### Phase 4 — Enhancement

| Component | Priority |
|-----------|----------|
| Reporting dashboard | Medium |
| In-app notifications (SignalR) | Medium |
| Audit logging | Medium |
| PWA support | Low |
| React Native mobile app | Low |
| Advanced analytics | Low |
