# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
dotnet build                                                    # Build full solution
dotnet test                                                     # Run all tests
dotnet test tests/TrainingOrganizer.Domain.Tests/               # Run domain tests
dotnet test tests/TrainingOrganizer.Application.Tests/          # Run application tests
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # Run single test
```

## Local Development (Docker)

```bash
docker compose -f docker/docker-compose.dev.yml up -d           # Start MongoDB + Keycloak only
docker compose -f docker/docker-compose.yml up --build           # Start full stack
```

## Architecture

Clean Architecture with DDD, targeting .NET 10.0 with MongoDB.

```
Api → Infrastructure → Application → Domain
```

- **Domain** — Pure domain layer, zero NuGet dependencies. Contains aggregates, entities, value objects, domain events, domain service interfaces, and exceptions.
- **Application** — CQRS via MediatR (commands/queries/handlers), FluentValidation, repository interfaces, DTOs, domain event handlers. References Domain.
- **Infrastructure** — MongoDB persistence (document-based mapping, repositories), Keycloak JWT auth, domain service implementations (RoomBookingService, SessionGenerationService, MemberUniquenessService). References Application.
- **Api** — ASP.NET Core minimal API endpoints, request/response contracts, exception-handling middleware. References Infrastructure.

## Domain Model (3 Bounded Contexts)

**Membership:** `Member` aggregate — registration, approval workflow, multi-role assignment (Member + Trainer + Admin simultaneously).

**Training:** `Training` (standalone), `RecurringTraining` (template), `TrainingSession` (generated occurrence) — participation, waitlists, capacity enforcement, attendance tracking. `ParticipantManager` is a shared internal helper for participation logic.

**Facility:** `Location` (with embedded `Room` entities), `Booking` — room management, time-slot booking, double-booking prevention.

Cross-aggregate communication uses domain events and eventual consistency. Aggregates reference each other by strongly-typed IDs only.

Full design spec: `docs/domain-model.md`
Architecture blueprint: `docs/architecture-blueprint.md`

## Application Layer Patterns

- **CQRS:** Commands return `Result` or `Result<T>`, queries return DTOs. All dispatched via MediatR `ISender`.
- **Command files:** Command record + handler + FluentValidation validator in the same file.
- **Domain event handlers:** Use `DomainEventNotification<T>` wrapper (since Domain doesn't reference MediatR). Implement `INotificationHandler<DomainEventNotification<TEvent>>`.
- **DI registration:** `services.AddApplication()` and `services.AddInfrastructure(configuration)`.

## Infrastructure Layer Patterns

- **Document pattern:** BSON-serializable document classes in `Persistence/Documents/` with `FromDomain()`/`ToDomain()` mapping methods — domain aggregates are never mapped directly by MongoDB.
- **Optimistic concurrency:** Repositories use `Version` field filter on `ReplaceOneAsync` — throws `ConcurrencyException` if stale.
- **MongoDB settings:** Configured via `MongoDB` section in appsettings (`ConnectionString`, `DatabaseName`).

## Code Conventions

- **Never use `= null!`** — use `required` keyword with `init` accessors instead. For mutable properties, use a backing field with `required ... init` on the public property and mutate via the backing field.
- Private constructors on aggregates/entities are marked with `[SetsRequiredMembers]`.
- CS8618 is suppressed project-wide in Domain (expected with this pattern).
- Value objects are `sealed record` types inheriting from `ValueObject`.
- Strongly-typed IDs inherit from `StronglyTypedId` (wraps `Guid`).
- Domain events are `sealed record` types implementing `IDomainEvent`.
- Aggregates use static factory methods (e.g., `Member.Register(...)`) — no public constructors.
- Enums for state machines: status transitions are guarded inside aggregate methods.
