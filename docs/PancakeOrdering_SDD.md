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

- Contracts defines the public API models, requests, responses, DTOs, and Result<T>.
- Core contains the Domain and Application logic.
- Infrastructure implements repositories and external-system ports.
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
SDD-3.3 Ingredient - an entity that can be added to Pancake
Contains:
- IngredientId
- Ingredient Name
- Ingredient Description

SDD-4. Public API Design
SDD-4.1 The Public API exposes application use cases without exposing Domain or Infrastructure types.
It contains only:
- Request models
- Response models
- DTOs
- Public identifiers
- Result<T>
SDD-4.2 Each API operation represents a clear business action.
Operations may be initiated by:
- A customer, for example creating, modifying, confirming, or cancelling an order.
- An external system, for example the Kitchen starting order preparation or the Delivery service updating delivery progress.
SDD-4.3 Application should not allow to change state directly, only API actions are allowed. All commands for the same order are submitted to an in-memory per-order queue and processed sequentially. 
SDD-4.4 Requests are validated at the API boundary before being passed to the Application layer. Business-rule and state-transition validation remains inside the Domain.
SDD-4.5 All operations return Result<T>:
- Success contains the requested response or DTO.
- Failure contains a stable error code and a readable message.
SDD-4.6 Domain and Infrastructure exceptions are not exposed to API consumers.
SDD-4.7 API models are mapped to and from Domain objects, keeping contract unchanged and keeping domain elements inside Application.

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
- Persisting the updated order
- Returning the result
Only then may the next command for the same order begin and validation will be performed vs updated state.
SDD-6.5 Commands for different orders use different queues and may execute concurrently.
SDD-6.6 The Order aggregate contains no locking or thread-synchronization logic.

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
