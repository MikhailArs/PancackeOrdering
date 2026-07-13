Pancake Ordering Design - Software Design Document (SDD)
Status: Initial Draft (changes may be done during implementation).


SDD-1. Purpose and Scope
The scope of the application is to implement a stateful logic for Pancake ordering.
Current application contains a core logic with no communication or persistence implemented
Using existing API application should be maintanable and allow easy integration with
 - Communication (with UI or another services)
 - Persistence (any DB or another service) 

SDD-2. Architectural Overview.
The solution follows a Clean Architecture structure:

- Contracts defines the public API models, requests, DTOs, public enums, OperationResult<T>, and OperationErrorCode.
- Core contains Domain logic only and has no reference to Contracts or Application.
- Application references Contracts and Core. It owns use cases, per-order command queues, ports, the public method-call facade, and mapping between Contracts and Domain.
- Infrastructure implements external adapters when required. The current Infrastructure adapter is an in-memory Kitchen.
- Host is the console demonstration composition root. It references Contracts and Infrastructure, and does not reference Core.
- Tests verify domain rules, application flows, and concurrency behavior.
- Demo may provide a simple executable example of the service.

SDD-3. Domain Model
SDD-3.1 Order - the Aggregate Root and is the only entry point for modifying an order.
Contains:
- OrderId
- Current order state
- Delivery address
- Collection of pancakes
SDD-3.2 Pancake - an entity identified by PancakeId and contains its selected ingredients
Contains:
- PancakeId is unique within its containing Order
- Collection of Ingredients
SDD-3.3 Ingredient is a supported ingredient type represented by an enum.
The currently supported values are:
- Honey
- Jam
- Chocolate

A Pancake may contain no additional ingredients.
Each ingredient type may occur at most once in a Pancake.
Ingredient stock and availability are separate Kitchen/Application concerns.
Stock quantities are not stored in Ingredient, Pancake, or Order.

SDD-4. Public API Design
SDD-4.1 The Public API exposes application use cases without exposing Domain or Infrastructure types.
It contains only:
- Request models
- DTOs
- Public identifiers
- Public enums
- OperationErrorCode
- OperationResult<T>
SDD-4.2 Each API operation represents a clear business action.
Operations may be initiated by:
- A customer, for example creating, modifying, confirming, or cancelling an order.
- An external system, for example the Kitchen starting order preparation or the Delivery service updating delivery progress.
SDD-4.3 Application should not allow to change state directly, only API actions are allowed. All commands for the same order are submitted to an in-memory per-order queue and processed sequentially. 
SDD-4.4 Requests are validated at the API boundary before being passed to the Application layer. Business-rule and state-transition validation remains inside the Domain.
SDD-4.5 All operations return OperationResult<T>:
- Success contains the requested DTO.
- Failure contains a stable public OperationErrorCode.
SDD-4.6 Domain and Infrastructure exceptions are not exposed to API consumers.
SDD-4.7 API models are mapped to and from Domain objects inside Application.
Contracts and Core do not reference each other.

SDD-4.8 The public C# method-call API is represented by IPancakeOrderingService in Contracts.
IPancakeOrderingService inherits the narrower IOrderQueryService contract so components that only need to read Orders can depend only on:
- GetOrder

It exposes Customer operations:
- CreateOrder
- GetOrder
- ChangeDeliveryAddressAsync
- AddPancakeAsync
- RemovePancakeAsync
- AddIngredientAsync
- RemoveIngredientAsync
- ConfirmOrderAsync
- CancelOrderAsync

Lifecycle operations for preparation, delivery, and archiving are also exposed on IPancakeOrderingService for integration and console demonstration purposes: StartPreparationAsync, CompletePreparationAsync, StartDeliveryAsync, CompleteDeliveryAsync, and ArchiveAsync.

SDD-4.9 Public callers use only request models, DTOs, primitive identifiers, public enums, OperationResult/OperationResult<T>, and OperationErrorCode from Contracts.
Domain objects such as Order, Pancake, DeliveryAddress, Ingredient, and state types do not leave Core.

SDD-4.10 Application maps Core Result and Core ErrorCode to Contracts OperationResult and OperationErrorCode explicitly.
The two result models are intentionally separate because they belong to different architectural boundaries.

SDD-4.11 OrderDto is a detached projection of an immutable internal Order snapshot containing OrderId, Status, DeliveryAddress, and Pancakes.
Customer commands are routed through the existing per-order command queue before returning an updated OrderDto.
Queued commands operate on the mutable Order aggregate and publish a complete immutable snapshot before the queued command completes.
GetOrder is synchronous, is not queued, reads only the current immutable snapshot, and returns the last completely published OrderDto.
In-progress command state is not exposed to queries.

The Kitchen uses the same public query model. When an Order is submitted to Kitchen, the Kitchen receives only OrderId and pulls the immutable OrderDto through IOrderQueryService. Domain objects, internal snapshots, and Kitchen-specific duplicate Order DTOs are not passed to Infrastructure.

SDD-5. Order Lifecycle Design
SDD-5.1 The order lifecycle is implemented using a lightweight State Pattern (see SDD-7. Trade-offs and Alternatives).
SDD-5.2 Each state implements IOrderState and defines:
- Operations allowed in that state
- Entry guards
- Local OnEnter and OnExit behavior
SDD-5.3 Transition rules are centralized to keep the lifecycle visible and consistent.

SDD-6. Command Processing and Concurrency
SDD-6.1 All commands for the same order are passed through a per-order command queue.
SDD-6.2 CreateOrder creates a new order in the Draft state and returns its OrderId. This command is the one and only which is not passed through the Order queue.
SDD-6.3 The processing flow is:
External API called --> Action is collected into Queue --> Action executed --> State Changed (Not always)
SDD-6.4 A command remains inside the queue until all related processing is complete, including:
- Validating
- Calling required external systems
- Applying the state transition
- Publishing a complete immutable Order snapshot
- Returning the result
Only then may the next command for the same order begin and validation will be performed vs updated state.
Snapshot publication replaces the whole immutable snapshot reference.
If a command mutates the Order and later returns a failure from an external boundary, the published snapshot still reflects the actual completed Aggregate state.
SDD-6.5 Commands for different orders use different queues and may execute concurrently.
SDD-6.6 The Order aggregate contains no locking or thread-synchronization logic.

SDD-6.7 The in-memory Kitchen protects its stock dictionary with one short private lock.

SDD-6.8 Kitchen and Ingredient Availability
SDD-6.8.1 Ingredient represented as an enum.
SDD-6.8.2 Draft AddPancake and AddIngredient commands check ingredient availability inside the same per-order Channel operation before mutating the Order. These checks do not reserve or consume stock.
SDD-6.8.3 Kitchen acceptance is the stock-consumption point. During Confirm, Application validates that confirmation is allowed, calls Kitchen with OrderId, and confirms the Order only after Kitchen accepts.

SDD-7. Trade-offs and Alternatives
SDD-7.1 State pattern vs state enum only
SDD-7.1.1 Enum with state
    Pros:
        - Easy to implement the Order class
    Cons:
        - Too many "If" and internal logic
        - Difficult to test
        - Too many critical sections for lock for thread safe
        - Complicated maintanability
        - Comlicated Transition rules

SDD-7.1.2 State Pattern
    Pros:
        - Clear Transition definition and maintanace
        - More testable
        - Clear and focused logic
        - Thread synch logic delegated to queue
    Cons:  
        - Design complexity     

SDD-7.2 Critical section management: Queue vs lock
SDD-7.2.1 Lock on critical section
    Pros:
        - Simple implementation
    Cons:
        - Lock should be set on non atomic actions such as external service call, this can cause deadlocks.
        - Multiple critical sections, one lock is not enouh, can cause deadlocks
        - Complicated testing
        - Complicated maintanance
SDD-7.2.2 Command Queue
    Pros:
        - State change sequentially, no race is possible
        - All thread sync logic is delegated to Queue and no critical sections are exists in order
        - Each next command receives updated state after all external service calls are done
        - Maintanable
        - Testable
    Cons:
        - Complicated implementation
