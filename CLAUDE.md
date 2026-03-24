# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
dotnet build                                                    # Build full solution
dotnet test                                                     # Run all tests
dotnet test tests/TrainingOrganizer.Membership.Tests/           # Run membership tests
dotnet test tests/TrainingOrganizer.Training.Tests/             # Run training tests
dotnet test tests/TrainingOrganizer.Facility.Tests/             # Run facility tests
dotnet test tests/TrainingOrganizer.UI.Tests/                   # Run UI tests (bunit)
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # Run single test
```

## Local Development (Docker)

```bash
docker compose -f docker/docker-compose.dev.yml up -d           # Start MongoDB + Keycloak only
docker compose -f docker/docker-compose.prod.yml up --build      # Start full stack
```

## Architecture

Vertical slice architecture by bounded context with DDD, targeting .NET 10.0 with MongoDB.

```
Api → Membership / Training / Facility → SharedKernel
Api → Adapter.EasyVerein / Adapter.Keycloak → Membership
```

- **SharedKernel** — Cross-cutting base types: `AggregateRoot`, `Entity`, `ValueObject`, `StronglyTypedId`, `IDomainEvent`, `TimeSlot`, domain exceptions, `Result<T>`, `PagedList<T>`, MediatR pipeline behaviors (validation, logging), `IUnitOfWork`, `IDateTimeProvider`, `ICurrentUserService`, `MongoDbContext`, `DomainObjectMapper`, `ConcurrencyException`.
- **Membership** — Bounded context slice: `Member` aggregate (domain), commands/queries/DTOs (application), MongoDB repository + `MemberUniquenessService` + `CurrentUserService` (infrastructure). All in one project with internal `Domain/Application/Infrastructure` folders.
- **Training** — Bounded context slice: `Training`, `RecurringTraining`, `TrainingSession` aggregates, `ParticipantManager`, commands/queries/DTOs, Schedule queries, `MemberSuspendedEventHandler` (cross-context consumer), MongoDB repositories + `SessionGenerationService`. References Membership (for `MemberId`) and Facility (for `RoomId`, `IRoomBookingService`).
- **Facility** — Bounded context slice: `Location` (with embedded `Room`), `Booking` aggregates, commands/queries/DTOs, MongoDB repositories + `RoomBookingService`.
- **Adapter.EasyVerein** — External service adapter for EasyVerein member import API. Implements `IEasyVereinApiClient`.
- **Adapter.Keycloak** — External service adapter for Keycloak admin API. Implements `IKeycloakAdminClient`.
- **Api** — ASP.NET Core minimal API endpoints, OpenAPI, exception-handling middleware. Composition root that wires all slices + adapters.
- **Shared** — API contracts (request/response records, enums) shared between backend and frontend.
- **UI** — Razor Class Library with Blazor pages, layouts, MudBlazor components, API client services. Shared by Web and Mobile hosts.
- **Web** — Blazor WebAssembly host with OIDC auth (Keycloak). Thin host around UI library.
- **Mobile** — MAUI Blazor Hybrid host (iOS/Android). Same UI components in native shell.

## Domain Model (3 Bounded Contexts)

**Membership:** `Member` aggregate — registration, approval workflow, multi-role assignment (Member + Trainer + Admin simultaneously).

**Training:** `Training` (standalone), `RecurringTraining` (template), `TrainingSession` (generated occurrence) — participation, waitlists, capacity enforcement, attendance tracking. `ParticipantManager` is a shared internal helper for participation logic.

**Facility:** `Location` (with embedded `Room` entities), `Booking` — room management, time-slot booking, double-booking prevention.

Cross-aggregate communication uses domain events and eventual consistency. Aggregates reference each other by strongly-typed IDs only.

Full design spec: `docs/domain-model.md`
Architecture blueprint: `docs/architecture-blueprint.md`

## Slice Project Structure

Each bounded context project uses internal folders:
```
TrainingOrganizer.{Context}/
  Domain/         (aggregates, entities, VOs, events, service interfaces)
  Application/    (commands, queries, handlers, DTOs, validators, repo interfaces)
  Infrastructure/ (documents, repositories, service implementations)
  DependencyInjection.cs
```

## Application Layer Patterns

- **CQRS:** Commands return `Result` or `Result<T>`, queries return DTOs. All dispatched via MediatR `ISender`.
- **Command files:** Command record + handler + FluentValidation validator in the same file.
- **Domain event handlers:** Use `DomainEventNotification<T>` wrapper (since Domain layer doesn't reference MediatR). Implement `INotificationHandler<DomainEventNotification<TEvent>>`.
- **DI registration:** Each slice has `AddMembership()`, `AddTraining()`, `AddFacility()`. SharedKernel has `AddSharedKernel(configuration)`. Adapters have `AddEasyVereinAdapter(configuration)`, `AddKeycloakAdapter(configuration)`.
- **MediatR scanning:** Program.cs explicitly scans all slice assemblies for handlers and validators.

## Infrastructure Layer Patterns

- **Document pattern:** BSON-serializable document classes in each slice's `Infrastructure/Persistence/Documents/` with `FromDomain()`/`ToDomain()` mapping methods — domain aggregates are never mapped directly by MongoDB.
- **DomainObjectMapper:** Public utility in SharedKernel for reconstructing domain objects via reflection (private constructors with `[SetsRequiredMembers]`).
- **MongoDbContext:** Generic context in SharedKernel exposing `IMongoDatabase Database`. Each repository accesses its own collection via `_context.Database.GetCollection<TDocument>("collection_name")`.
- **Optimistic concurrency:** Repositories use `Version` field filter on `ReplaceOneAsync` — throws `ConcurrencyException` if stale.
- **MongoDB settings:** Configured via `MongoDB` section in appsettings (`ConnectionString`, `DatabaseName`).

## Code Conventions

- **Never use `= null!`** — use `required` keyword with `init` accessors instead. For mutable properties, use a backing field with `required ... init` on the public property and mutate via the backing field.
- Private constructors on aggregates/entities are marked with `[SetsRequiredMembers]`.
- CS8618 is suppressed project-wide in slice projects (expected with this pattern).
- Value objects are `sealed record` types inheriting from `ValueObject`.
- Strongly-typed IDs inherit from `StronglyTypedId` (wraps `Guid`).
- Domain events are `sealed record` types implementing `IDomainEvent`.
- Aggregates use static factory methods (e.g., `Member.Register(...)`) — no public constructors.
- Enums for state machines: status transitions are guarded inside aggregate methods.
- `ICurrentUserService.MemberId` returns `Guid?` (not strongly-typed `MemberId`) to avoid circular dependency between SharedKernel and Membership. Callers wrap with `new MemberId(guid)` where needed.
