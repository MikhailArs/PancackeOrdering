# Pancake Ordering Traceability

Requirement IDs are taken from `docs/PancakeOrdering_Requirements.md`. The requirements file writes NFR identifiers as `NFR 1`, `NFR 2`, etc.; this matrix uses the canonical metadata form `NFR-1`, `NFR-2`, etc.

## Requirement Matrix

| Requirement | Requirement Summary | Design | Tests | Coverage |
| ----------- | ------------------- | ------ | ----- | -------- |
| FR-1 | Create an Order with a valid Delivery Address | SDD-3.1, SDD-4.8, SDD-4.9, SDD-4.11, SDD-6.2 | TST-04.01, TST-04.06, TST-05.05, TST-06.09 | Covered |
| FR-2 | Change Delivery Address before cancelled or confirmed | SDD-4.8, SDD-5.2 | TST-05.11 | Covered |
| FR-3 | Confirm an Order that contains at least one Pancake | SDD-4.8, SDD-5.2, SDD-6.4, SDD-6.8.3 | TST-04.03, TST-04.04, TST-04.05, TST-04.06, TST-05.12, TST-05.22, TST-06.04, TST-06.06, TST-06.09 | Covered |
| FR-4 | Submit confirmed Order to Kitchen for preparation | SDD-4.11, SDD-6.4, SDD-6.8.3 | TST-04.03, TST-04.05, TST-04.06, TST-05.12, TST-05.22, TST-06.03, TST-06.04, TST-06.05, TST-06.06, TST-06.07, TST-06.08, TST-06.09 | Covered |
| FR-5 | Submit Order for delivery after Kitchen preparation | SDD-4.2, SDD-4.11, SDD-5.3, SDD-6.4 | TST-04.06, TST-04.07, TST-04.08, TST-04.09, TST-04.10, TST-04.11, TST-05.15, TST-05.16, TST-05.22, TST-06.09 | Covered |
| FR-6 | Archive Order after delivery | SDD-4.2, SDD-5.3, SDD-6.4 | TST-04.06, TST-04.12, TST-04.13, TST-04.14, TST-04.15, TST-04.16, TST-05.22, TST-06.09 | Covered |
| FR-7 | Cancel Order before Kitchen preparation starts | SDD-4.8, SDD-5.3 | TST-05.13 | Covered |
| FR-8 | Add Pancakes before cancelled or confirmed | SDD-4.8, SDD-4.9, SDD-6.8.2 | TST-05.07, TST-05.08, TST-05.09, TST-06.01, TST-06.02, TST-06.10 | Covered |
| FR-9 | Add or remove Ingredients before confirmed or cancelled | SDD-3.3, SDD-4.8, SDD-4.11, SDD-6.4, SDD-6.8.2 | TST-05.08, TST-05.10, TST-05.17, TST-06.01, TST-06.02, TST-06.10 | Covered |
| FR-10 | Verify Ingredient availability | SDD-3.3, SDD-4.11, SDD-6.7, SDD-6.8.2, SDD-6.8.3 | TST-05.02, TST-05.03, TST-05.04, TST-06.01, TST-06.02, TST-06.04, TST-06.05, TST-06.06, TST-06.07, TST-06.08, TST-06.09, TST-06.10 | Covered |
| NFR-1 | Written in Java or C# | None | None | Not Applicable |
| NFR-2 | Expose API as Java or C# method calls | SDD-4.8, SDD-4.9, SDD-4.11 | TST-05.05, TST-05.06, TST-05.15, TST-05.21, TST-05.22 | Covered |
| NFR-3 | Do not expose internal domain objects via API | SDD-2, SDD-4.1, SDD-4.7, SDD-4.9, SDD-4.11 | TST-05.01, TST-05.03, TST-05.05, TST-05.07, TST-05.08, TST-05.17, TST-05.18, TST-05.19, TST-05.20, TST-05.22, TST-06.03 | Covered |
| NFR-4 | Do not throw exceptions | SDD-4.5, SDD-4.6 | TST-03.02, TST-04.02, TST-05.06, TST-05.14 | Covered |
| NFR-5 | Avoid undefined behavior from race conditions | SDD-4.3, SDD-6.1, SDD-6.3, SDD-6.4, SDD-6.5, SDD-6.7, SDD-7.2.2 | TST-03.01, TST-03.02, TST-04.06, TST-04.10, TST-04.16, TST-04.17, TST-04.18, TST-04.19, TST-05.15, TST-05.16, TST-05.17, TST-06.08 | Covered |
| NFR-6 | Follow OOP and SOLID principles for future adaptation | SDD-2, SDD-4.7 | TST-05.01 | Covered |
| NFR-7 | Be properly tested | SDD-2 | Full test catalog | Covered |
| NFR-8 | Use no frameworks or libraries except for unit tests | None | None | Not Covered |

## Test Catalog

`TST-02.08` and `TST-02.09` are parameterized test-case groups and are listed once per NUnit method.

| Test ID | Test Name | Suite | Requirements | Design |
| ------- | --------- | ----- | ------------ | ------ |
| TST-01.01 | Confirm_WithOnePancake_ChangesStatusToConfirmed | TST-01 | None | SDD-3.1, SDD-5.2 |
| TST-01.02 | Confirm_WithoutPancakes_ReturnsFailureAndKeepsDraftStatus | TST-01 | None | SDD-3.1, SDD-5.2 |
| TST-01.03 | AddPancake_OnDraftState_PancakeAdded | TST-01 | None | SDD-3.1, SDD-3.2, SDD-5.2 |
| TST-01.04 | RemovePancake_OnDraftState_PancakeRemoved | TST-01 | None | SDD-3.1, SDD-3.2, SDD-5.2 |
| TST-01.05 | RemoveNonExistingPancake_OnDraftState_PancakeAdded | TST-01 | None | SDD-3.1, SDD-3.2 |
| TST-01.06 | RemovePancake_OnConfirmedState_ReturnsFailure | TST-01 | None | SDD-3.1, SDD-3.2, SDD-5.2 |
| TST-01.07 | AddPancake_OnConfirmedState_ReturnsFailure | TST-01 | None | SDD-3.1, SDD-3.2, SDD-5.2 |
| TST-01.08 | AddIngredient_OnDraftState_AddsIngredient | TST-01 | None | SDD-3.2, SDD-3.3, SDD-5.2 |
| TST-01.09 | AddIngredient_WhenAlreadyExists_ReturnsFailure | TST-01 | None | SDD-3.2, SDD-3.3 |
| TST-01.10 | RemoveIngredient_OnDraftState_RemovesIngredient | TST-01 | None | SDD-3.2, SDD-3.3, SDD-5.2 |
| TST-01.11 | RemoveIngredient_WhenIngredientDoesNotExist_ReturnsFailure | TST-01 | None | SDD-3.2, SDD-3.3 |
| TST-01.12 | ModifyIngredients_OnConfirmedOrder_ReturnsFailure | TST-01 | None | SDD-3.2, SDD-3.3, SDD-5.2 |
| TST-01.13 | CreateOrder_WithValidAddress_AddressStored | TST-01 | None | SDD-3.1 |
| TST-01.14 | ChangeAddress_OnDraftState_AddressChanged | TST-01 | None | SDD-3.1, SDD-5.2 |
| TST-01.15 | ChangeAddress_OnConfirmedOrder_ReturnsFailure | TST-01 | None | SDD-3.1, SDD-5.2 |
| TST-01.16 | CreateAddress_WithNullField_ReturnsFailure | TST-01 | None | SDD-3.1 |
| TST-01.17 | ChangeAddress_WithInvalidAddress_ReturnsFailureAndKeepsCurrentAddress | TST-01 | None | SDD-3.1, SDD-5.2 |
| TST-02.01 | FullValidLifecycle_ReachesArchived | TST-02 | None | SDD-5.1, SDD-5.2, SDD-5.3 |
| TST-02.02 | Cancel_FromDraft_ChangesStatusToCancelled | TST-02 | None | SDD-5.2, SDD-5.3 |
| TST-02.03 | Cancel_FromConfirmed_ChangesStatusToCancelled | TST-02 | None | SDD-5.2, SDD-5.3 |
| TST-02.04 | StartPreparation_FromDraft_ReturnsInvalidTransitionAndKeepsDraft | TST-02 | None | SDD-5.2, SDD-5.3 |
| TST-02.05 | Confirm_FromConfirmed_ReturnsInvalidTransitionAndKeepsConfirmed | TST-02 | None | SDD-5.2, SDD-5.3 |
| TST-02.06 | LifecycleOperation_FromCancelled_ReturnsInvalidTransitionAndKeepsCancelled | TST-02 | None | SDD-5.2, SDD-5.3 |
| TST-02.07 | LifecycleOperation_FromArchived_ReturnsInvalidTransitionAndKeepsArchived | TST-02 | None | SDD-5.2, SDD-5.3 |
| TST-02.08 | TransitionRules_ValidPairsResolveExpectedTarget | TST-02 | None | SDD-5.3 |
| TST-02.09 | TransitionRules_InvalidPairsDoNotResolve | TST-02 | None | SDD-5.3 |
| TST-03.01 | EnqueueAsync_ExecutesOperationsInFifoOrder | TST-03 | NFR-5 | SDD-6.1, SDD-6.3, SDD-7.2.2 |
| TST-03.02 | EnqueueAsync_ExceptionDuringFirstTask_ReturnsInternalErrorAndSecondContinues | TST-03 | NFR-4, NFR-5 | SDD-4.6, SDD-6.1, SDD-6.3 |
| TST-04.01 | CreateOrder_ReturnsStableNonEmptyUniqueIds | TST-04 | FR-1 | SDD-3.1, SDD-6.2 |
| TST-04.02 | Command_WithUnknownOrder_ReturnsOrderNotFound | TST-04 | NFR-4 | SDD-4.5, SDD-4.6 |
| TST-04.03 | Confirm_WhenKitchenAccepts_ConfirmsOrder | TST-04 | FR-3, FR-4 | SDD-6.4, SDD-6.8.3 |
| TST-04.04 | Confirm_WhenDraftOrderIsEmpty_DoesNotCallKitchen | TST-04 | FR-3 | SDD-5.2, SDD-6.8.3 |
| TST-04.05 | Confirm_WhenKitchenDeclines_KeepsOrderDraft | TST-04 | FR-3, FR-4 | SDD-6.4, SDD-6.8.3 |
| TST-04.06 | MainLifecycle_WithAcceptedKitchen_ReachesArchived | TST-04 | FR-1, FR-3, FR-4, FR-5, FR-6, NFR-5 | SDD-5.3, SDD-6.4, SDD-6.8.3 |
| TST-04.07 | CompletePreparation_WhenValid_SubmitsOrderToDelivery | TST-04 | FR-5 | SDD-4.2, SDD-6.4 |
| TST-04.08 | CompletePreparation_WhenTransitionIsInvalid_DoesNotCallDelivery | TST-04 | FR-5 | SDD-5.3, SDD-6.4 |
| TST-04.09 | CompletePreparation_WhenDeliverySubmissionFails_LeavesOrderPrepared | TST-04 | FR-5 | SDD-6.4 |
| TST-04.10 | CompletePreparation_WaitsForDeliverySubmissionBeforeNextOrderCommand | TST-04 | FR-5, NFR-5 | SDD-6.1, SDD-6.4 |
| TST-04.11 | CompleteDelivery_WhenValid_EndsInDelivered | TST-04 | FR-5 | SDD-5.3 |
| TST-04.12 | CompleteDelivery_WhenTransitionIsInvalid_DoesNotArchive | TST-04 | FR-6 | SDD-5.3, SDD-6.4 |
| TST-04.13 | Archive_WhenGatewaySucceeds_ReachesArchived | TST-04 | FR-6 | SDD-4.2, SDD-6.4 |
| TST-04.14 | Archive_WhenTransitionIsInvalid_DoesNotCallGateway | TST-04 | FR-6 | SDD-5.3, SDD-6.4 |
| TST-04.15 | Archive_WhenGatewayFails_LeavesOrderDelivered | TST-04 | FR-6 | SDD-6.4 |
| TST-04.16 | Archive_WaitsForGatewayBeforeNextOrderCommand | TST-04 | FR-6, NFR-5 | SDD-6.1, SDD-6.4 |
| TST-04.17 | SameOrder_WaitsForCompletePreviousCommandIncludingKitchen | TST-04 | NFR-5 | SDD-6.1, SDD-6.4, SDD-6.8.3, SDD-7.2.2 |
| TST-04.18 | DifferentOrders_ExecuteConcurrently | TST-04 | NFR-5 | SDD-6.5 |
| TST-04.19 | CompetingCustomerAndKitchenCommands_FollowEnqueueOrder | TST-04 | NFR-5 | SDD-6.1, SDD-6.4, SDD-7.2.2 |
| TST-05.01 | ProjectReferences_FollowRequiredDependencyDirection | TST-05 | NFR-3, NFR-6 | SDD-2, SDD-4.7 |
| TST-05.02 | PublicConstructor_DoesNotComposeApplicationPorts | TST-05 | FR-10 | SDD-6.8.2 |
| TST-05.03 | KitchenGateway_DoesNotNeedToImplementIngredientAvailability | TST-05 | FR-10, NFR-3 | SDD-4.11, SDD-6.8.2 |
| TST-05.04 | AvailabilityFailure_IsRespectedBeforeDraftMutation | TST-05 | FR-10 | SDD-6.8.2 |
| TST-05.05 | CreateOrder_ReturnsDraftOrderDtoAndGetOrderReturnsSameOrder | TST-05 | FR-1, NFR-2, NFR-3 | SDD-4.8, SDD-4.9, SDD-4.11, SDD-6.2 |
| TST-05.06 | GetOrder_WhenOrderDoesNotExist_ReturnsTypedOrderNotFound | TST-05 | NFR-2, NFR-4 | SDD-4.5, SDD-4.8, SDD-4.11 |
| TST-05.07 | AddPancake_WithNoIngredients_ReturnsOrderDtoWithEmptyPancake | TST-05 | FR-8, NFR-3 | SDD-4.8, SDD-4.9 |
| TST-05.08 | AddPancake_WithIngredients_MapsIngredientsToDto | TST-05 | FR-8, FR-9, NFR-3 | SDD-4.8, SDD-4.9, SDD-6.8.2 |
| TST-05.09 | RemovePancake_ReturnsOrderDtoWithoutRemovedPancake | TST-05 | FR-8 | SDD-4.8 |
| TST-05.10 | AddAndRemoveIngredient_ReturnUpdatedOrderDto | TST-05 | FR-9 | SDD-3.3, SDD-4.8 |
| TST-05.11 | ChangeDeliveryAddress_ReturnsUpdatedAddressAndFailsAfterConfirmation | TST-05 | FR-2 | SDD-4.8, SDD-5.2 |
| TST-05.12 | ConfirmOrder_WithAcceptingKitchen_ReturnsConfirmedOrderDto | TST-05 | FR-3, FR-4 | SDD-4.8, SDD-6.8.3 |
| TST-05.13 | CancelOrder_WhenValid_ReturnsCancelledOrderDto | TST-05 | FR-7 | SDD-4.8, SDD-5.3 |
| TST-05.14 | AddPancake_WithInvalidPublicIngredient_ReturnsTypedFailureAndLeavesOrderUnchanged | TST-05 | NFR-4 | SDD-4.4, SDD-4.5 |
| TST-05.15 | GetOrder_DuringIncompleteCommand_ReturnsPreviousPublishedSnapshot | TST-05 | FR-5, NFR-2, NFR-5 | SDD-4.11, SDD-6.4 |
| TST-05.16 | GetOrder_AfterFailedCommandThatMutatedOrder_ReturnsUpdatedSnapshot | TST-05 | FR-5, NFR-5 | SDD-4.11, SDD-6.4 |
| TST-05.17 | PreviouslyReturnedDto_DoesNotChangeAfterLaterCommand | TST-05 | FR-9, NFR-3, NFR-5 | SDD-4.11, SDD-6.4 |
| TST-05.18 | OrderDto_CollectionsAreRuntimeReadOnlyCollections | TST-05 | NFR-3 | SDD-4.9, SDD-4.11 |
| TST-05.19 | ApplicationSnapshotQuery_DoesNotReturnMutableOrder | TST-05 | NFR-3 | SDD-4.11 |
| TST-05.20 | PublicContractsGraph_DoesNotExposeCoreTypes | TST-05 | NFR-3 | SDD-4.1, SDD-4.9 |
| TST-05.21 | GetOrder_IsSynchronous | TST-05 | NFR-2 | SDD-4.8, SDD-4.11 |
| TST-05.22 | LifecycleMethods_ReturnUpdatedOrderDtosAndReachArchived | TST-05 | FR-3, FR-4, FR-5, FR-6, NFR-2, NFR-3 | SDD-4.8, SDD-5.3, SDD-6.4 |
| TST-06.01 | DraftAvailability_DoesNotConsumeStock | TST-06 | FR-8, FR-9, FR-10 | SDD-3.3, SDD-6.8.2 |
| TST-06.02 | UnavailableIngredient_PreventsDraftModification | TST-06 | FR-8, FR-9, FR-10 | SDD-3.3, SDD-6.8.2 |
| TST-06.03 | AcceptOrder_PullsOrderDtoByOrderId | TST-06 | FR-4, NFR-3 | SDD-4.11, SDD-6.8.3 |
| TST-06.04 | Confirm_WhenKitchenAccepts_ConsumesStock | TST-06 | FR-3, FR-4, FR-10 | SDD-6.8.3 |
| TST-06.05 | InMemoryKitchen_MayProvideKitchenAndAvailabilityCapabilities | TST-06 | FR-4, FR-10 | SDD-6.8.2, SDD-6.8.3 |
| TST-06.06 | Confirm_WhenKitchenRunsOutOfStock_DeclinesWithoutNegativeInventory | TST-06 | FR-3, FR-4, FR-10 | SDD-6.8.3 |
| TST-06.07 | AcceptOrder_WhenOneIngredientIsUnavailable_DeductsNothing | TST-06 | FR-4, FR-10 | SDD-6.8.3 |
| TST-06.08 | ConcurrentConfirmations_CannotOverConsumeStock | TST-06 | FR-4, FR-10, NFR-5 | SDD-6.7, SDD-6.8.3 |
| TST-06.09 | MainFlow_WithEnoughKitchenStock_ReachesArchived | TST-06 | FR-1, FR-3, FR-4, FR-5, FR-6, FR-10 | SDD-5.3, SDD-6.4, SDD-6.8.3 |
| TST-06.10 | RemovingDraftItems_DoesNotRestoreStock | TST-06 | FR-8, FR-9, FR-10 | SDD-6.8.2 |
