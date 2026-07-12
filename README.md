# Pancake Ordering Service

A C# implementation of a pancake ordering workflow demonstrating:

* Clean Architecture
* Rich Domain Model
* State Pattern
* Producer–Consumer Pattern
* race-free command processing
* typed Result-based error handling
* testability and requirement traceability

## Architecture

```text
Contracts ← Application → Core
                 ↓
          Infrastructure
```

### Contracts

Contains the public C# method-call API:

* requests
* DTOs
* public enums
* `OperationResult`
* `OperationResult<T>`
* `OperationErrorCode`

Contracts has no dependency on Core.

### Core

Contains the Domain model:

* `Order` Aggregate Root
* `Pancake`
* `DeliveryAddress`
* ingredient types
* lifecycle states
* centralized transition rules
* internal `Result` model

Core has no project dependencies.

### Application

Contains:

* use-case orchestration
* public API implementation
* DTO mapping
* immutable Order snapshots
* per-order command queues
* Kitchen, Delivery, Archive, and availability ports

### Infrastructure

Contains concrete adapters such as the in-memory Kitchen inventory.

## Order Lifecycle

```text
Draft
 ├── Confirm → Confirmed
 └── Cancel → Cancelled

Confirmed
 ├── StartPreparation → Preparing
 └── Cancel → Cancelled

Preparing → Prepared
Prepared → OutForDelivery
OutForDelivery → Delivered
Delivered → Archived
```

`Cancelled` and `Archived` are terminal states.

All transitions use one centralized transition mechanism:

```text
resolve rule
→ validate target state
→ OnExit
→ change state
→ OnEnter
```

## Concurrency

Each Order has its own unbounded `Channel`.

```text
Multiple producers
→ per-order FIFO Channel
→ one async consumer
→ Order Aggregate
```

Commands for the same Order execute sequentially.

Commands for different Orders may execute concurrently.

No locks are used inside the Order aggregate.

## Commands and Queries

State-changing commands use the per-order Channel.

`GetOrder` is synchronous and reads the latest immutable Order snapshot.

A query never reads the mutable Aggregate directly and never exposes an in-progress command state.

## Kitchen Inventory

Ingredients are represented by an enum:

```text
Honey
Jam
Chocolate
```

Stock quantities belong to Kitchen, not to the Domain Order.

Draft availability checks do not consume stock.

Stock is consumed only when Kitchen accepts the complete Order.

Kitchen receives only `OrderId`, queries `OrderDto`, and atomically checks and decrements inventory.

A short private `lock` protects the shared inventory across different Orders.

## External Flows

```text
Confirm
→ Kitchen accepts
→ Confirmed

CompletePreparation
→ Prepared
→ submitted to Delivery

CompleteDelivery
→ Delivered

Archive service accepts
→ Archived
```

Kitchen, Delivery, and Archive never receive Domain objects.

## Testing

NUnit tests cover:

* Domain invariants
* valid and invalid transitions
* FIFO command execution
* competing Customer and Kitchen commands
* concurrent processing of different Orders
* immutable snapshots
* Kitchen inventory contention
* public API mapping
* prevention of Domain type leakage

Tests use requirement metadata:

```csharp
[Property("Requirement", "FR-10")]
```

## Build

```bash
dotnet restore
dotnet build
dotnet test
```

## Intentional Limitations

Not implemented because they are outside the assignment scope:

* database persistence
* HTTP or gRPC transport
* distributed transactions
* Outbox or Saga
* inventory reservations
* retries
* queue cleanup
* production logging
