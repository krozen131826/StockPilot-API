# StockPilot Purchase-Order Workflow

## 1. Overview

This document defines the purchase-order lifecycle and business rules for StockPilot.

The purchase-order workflow manages orders placed with suppliers, from initial preparation through submission and receipt of goods.

The MVP supports three purchase-order statuses:

- `Draft`
- `Submitted`
- `Received`

Partial receiving and purchase-order cancellation are outside the current scope.

## 2. Purchase-Order Lifecycle

A purchase order follows this sequence:

**Draft → Submitted → Received**

Each transition must follow the business rules defined below. A purchase order cannot skip a status or return to an earlier status through the normal workflow.

## 3. Purchase-Order Statuses

### 3.1 Draft

A purchase order begins in the Draft status when it is created.

**Allowed operations**
- Add purchase-order items.
- Update purchase-order items.
- Remove purchase-order items.
- Update permitted order details.
- Submit the order when all submission requirements are satisfied.

**Business rules**
- The order must reference a valid supplier.
- Each order line must reference a valid product.
- Ordered quantities must be greater than zero.
- Unit costs cannot be negative.
- The order must contain at least one valid item before submission.
- Creating or editing a Draft order must not change inventory quantities.

### 3.2 Submitted

A purchase order enters the Submitted status after a successful submission.

The order has been submitted to the supplier and is awaiting delivery.

**Allowed operations**
- View the order and its items.
- Receive the order when the goods arrive.

**Business rules**
- The order must have at least one valid item before submission.
- The submission timestamp must be recorded.
- The order is locked from editing in the MVP.
- Submitting the order must not change inventory quantities.
- Only a Submitted order can be received.

### 3.3 Received

A purchase order enters the Received status after the goods have been received and inventory has been updated successfully.

**Business rules**
- Only a Submitted order can be received.
- The order's products must have their stock quantities increased by the received quantities.
- A StockMovement audit record must be created for each received product line.
- The receipt timestamp must be recorded.
- The order status, inventory changes, and stock movement records must be saved in one database transaction.
- The order cannot be received a second time.
- The order cannot be edited through the normal workflow after receipt.

## 4. Allowed State Transitions

### Draft → Submit
- **Condition:** The order contains at least one valid order item.
- **Result:** The order status changes to `Submitted`.

### Submitted → Receive
- **Condition:** The order is valid and has not already been received.
- **Result:** Inventory quantities are updated, stock movements are recorded, and the order status changes to `Received`.

### Draft → Receive
- **Allowed:** No.
- **Result:** The request is rejected because the order must be submitted before it can be received.

### Submitted → Submit Again
- **Allowed:** No.
- **Result:** The request is rejected because the order has already been submitted.

### Received → Receive Again
- **Allowed:** No.
- **Result:** The request is rejected because the order has already been received.
- **Result:** The request is rejected because the order has already been received.
- **Result:** The request is rejected because the order has already been received.
- **Result:** The request is rejected because the order has already been received.

### Submitted → Edit
- **Allowed:** No, not in the current MVP.
- **Result:** The request is rejected because submitted orders are locked from editing.

### Received → Edit
- **Allowed:** No, not in the current MVP.
- **Result:** The request is rejected because received orders cannot be edited through the normal workflow.


## 5. Submitting a Purchase Order

When a user submits a Draft purchase order, the application must:

1. Load the purchase order and its items.
2. Confirm that the order exists and is in Draft status.
3. Validate the supplier and order items.
4. Confirm that the order contains at least one valid item.
5. Validate each item's product, quantity, and unit cost.
6. Change the status to Submitted.
7. Set `SubmittedAt`.
8. Save the changes.

Submitting an order must not update product stock quantities or create stock movements.

## 6. Receiving a Purchase Order

When a user receives a Submitted purchase order, the application must perform the operation within a single database transaction.

### Processing steps

1. Load the purchase order and its items.
2. Confirm that the order exists and its status is Submitted.
3. Validate the order lines and products.
4. For each order line:
   - Read the current product stock quantity.
   - Calculate the new quantity by adding the received quantity.
   - Update the product's `QuantityOnHand`.
   - Create a StockMovement record.
5. Set the purchase-order status to Received.
6. Set `ReceivedAt`.
7. Save the changes and commit the transaction.

### StockMovement details

Each stock movement created by receiving goods must record:

- `ProductId`
- `PurchaseOrderId`
- `MovementType` as `StockIn`
- `QuantityChange`
- `QuantityBefore`
- `QuantityAfter`
- `Reason`
- `CreatedAt`

The following relationship must hold:

`QuantityAfter = QuantityBefore + QuantityChange`

### Transaction safety

All receipt-related changes must succeed or fail together.

If an error occurs while updating inventory, creating stock movements, or changing the purchase-order status, the transaction must roll back.

This prevents inconsistent data, such as inventory being increased while the purchase order remains Submitted.

The application must also prevent concurrent requests from receiving the same order more than once.

## 7. Inventory Rules

The following rules apply throughout the purchase-order lifecycle:

- Creating a Draft order does not affect inventory.
- Editing a Draft order does not affect inventory.
- Submitting an order does not affect inventory.
- Receiving a Submitted order increases inventory.
- Stock quantities cannot become negative.
- Every stock change caused by receiving goods must have a corresponding StockMovement record.
- StockMovement records are audit history and must not be edited or deleted through ordinary application operations.

## 8. Partial Receiving

Partial receiving is not supported in the current MVP.

A Submitted order is received as one complete operation. For a successful receipt, each item's `QuantityReceived` should match its `QuantityOrdered`.

The system must not mark an order as Received while only part of its ordered quantity has been recorded.

## 9. Authorization

The API must enforce authorization for purchase-order operations.

The user interface must not be the only place where permissions are enforced.

Operations must follow the agreed permissions for the Administrator and Staff roles. Role permissions should be explicitly defined rather than assumed.

## 10. Error Handling

The application must reject invalid operations and return clear error responses.

Examples include:

- Attempting to submit an order that is not in Draft status.
- Attempting to submit an order with no items.
- Attempting to receive an order that is still in Draft status.
- Attempting to receive an order that has already been received.
- Attempting to edit a Submitted or Received order.
- Attempting to receive an order with invalid quantities or invalid product references.

## 11. Out of Scope

The current purchase-order workflow does not include:

- Partial receiving.
- Purchase-order cancellation.
- Reopening submitted orders.
- Editing received orders.
- Automatic inventory changes when an order is created or submitted.
- Multiple warehouses.
- Supplier payment processing.

## 12. Maintenance

Update this document whenever an agreed change modifies the purchase-order statuses, transitions, or business rules.

Keep this document aligned with the actual domain entities, application logic, and EF Core configuration.
