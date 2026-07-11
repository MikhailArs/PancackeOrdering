Functional Requirements (FR)

FR-1 - The Customer shall be able to create a new Order with a valid Delivery Address.
FR-2 - The Customer shall be able to change the Delivery Address to another valid Delivery Address
before the Order has been cancelled or confirmed.
FR-3 - The Customer shall be able to confirm the Order if at least one Pancake has been added to the
Order.
FR-4 - The Software shall submit the Order to the Kitchen for preparation after the Customer has
confirmed the Order.
FR-5 - The Software shall submit the Order for delivery after it has been prepared by the Kitchen.
FR-6 - The Software shall archive the Order after the Order has been delivered.
FR-7 - The Customer shall be able to cancel the order if the Kitchen has not started to prepare it.
FR-8 - The Customer shall be able to add Pancakes to the Order if the Order has not been cancelled or
confirmed.
FR-9 - The Customer shall be able to add or remove Ingredients from any Pancake in the Order at any
time before the Order has been confirmed or cancelled.
FR-10 - The Software shall verify the availability of Ingredients added to Pancakes. 


Non-Functional Requirements (NFR)

NFR 1 - The Software shall be written in Java or C#.
NFR 2 - The Software shall expose the API in the from of Java or C# method calls.
NFR 3 - The Software shall not expose internal domain objects via the API.
NFR 4 - The Software shall not throw exceptions.
NFR 5 - The Software shall ensure no undefined behaviour shall happen due to race conditions.
NFR 6 - The Software shall be written following OOP and SOLID principles to simplify future adaptation
to changes in functional requirements.
NFR 7 - The Software shall be properly tested.
NFR 8 - The Software shall not use any frameworks or libraries except for writing Unit Tests, it shall be
based on core language features. 


Assumptions

- The implementation uses in-memory persistence. 
- Archiving is represented as a terminal lifecycle state.
- Ingredient availability is provided through an external abstraction.
- Commands for different orders may execute concurrently.
- Commands for the same order are processed in enqueue order.
- Ingredients of same type can not be added in same pancake


Out of Scope
- Persistanse EF or other ORM are out of scope, but should be easy to add any
- Communication or DTOs serialization (JSON/Protobuf) is out of scope, but should be easy to add any