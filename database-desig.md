# StockPilot Database Design

## 1. Overview

StockPilot is an inventory and purchasing management application designed for a single business.

The database manages product categories, products, suppliers, purchase orders, and stock movement history.

The application uses PostgreSQL as its database and Entity Framework Core for data access.

## 2. Entities

### 2.1 Category

Represents a category used to organize products.

**Properties**
- `Id` — Primary key.
- `Name` — Required category name, maximum 100 characters.
- `Description` — Optional description.
- `IsActive` — Indicates whether the category is active.
- `CreatedAt` — Date and time the record was created.
- `UpdatedAt` — Date and time the record was last updated, if applicable.

**Relationships**
- One Category can have many Products.
- Each Product belongs to one Category.

### 2.2 Product

Represents an inventory item managed by StockPilot.

**Properties**
- `Id` — Primary key.
- `CategoryId` — Foreign key referencing Category.
- `SKU` — Unique stock-keeping unit.
- `Name` — Product name.
- `Description` — Optional product description.
- `CostPrice` — Cost per unit.
- `SellingPrice` — Selling price per unit.
- `QuantityOnHand` — Current available stock quantity.
- `ReorderLevel` — Quantity threshold used to identify low-stock products.
- `IsActive` — Indicates whether the product is active.
- `CreatedAt` — Date and time the record was created.
- `UpdatedAt` — Date and time the record was last updated, if applicable.

**Relationships**
- Each Product belongs to one Category.
- One Product can appear in many PurchaseOrderItems.
- One Product can have many StockMovements.

**Business Rules**
- SKU must be unique, case-insensitively.
- CostPrice and SellingPrice cannot be negative.
- QuantityOnHand cannot be negative.
- ReorderLevel cannot be negative.
- An active product is considered low-stock when QuantityOnHand is less than or equal to ReorderLevel.

### 2.3 Supplier

Represents a supplier from whom the business purchases products.

**Properties**
- `Id` — Primary key.
- `Name` — Required supplier name.
- `ContactPerson` — Optional contact person.
- `Email` — Optional email address.
- `Phone` — Optional phone number.
- `Address` — Optional supplier address.
- `IsActive` — Indicates whether the supplier is active.
- `CreatedAt` — Date and time the record was created.
- `UpdatedAt` — Date and time the record was last updated, if applicable.

**Relationships**
- One Supplier can have many PurchaseOrders.
- Each PurchaseOrder belongs to one Supplier.

### 2.4 PurchaseOrder

Represents an order placed with a supplier.

**Properties**
- `Id` — Primary key.
- `SupplierId` — Foreign key referencing Supplier.
- `OrderNumber` — Unique purchase-order identifier.
- `Status` — Current order status.
- `SubmittedAt` — Date and time the order was submitted, if applicable.
- `ReceivedAt` — Date and time the order was received, if applicable.
- `Notes` — Optional order notes.
- `CreatedAt` — Date and time the record was created.
- `UpdatedAt` — Date and time the record was last updated, if applicable.

**Relationships**
- Each PurchaseOrder belongs to one Supplier.
- One PurchaseOrder has many PurchaseOrderItems.
- A PurchaseOrder can be referenced by StockMovements created when its goods are received.

**Purchase-Order Statuses**
- `Draft` — The order is being prepared.
- `Submitted` — The order has been submitted to the supplier and is awaiting delivery.
- `Received` — The ordered goods have been received and inventory has been updated.

### 2.5 PurchaseOrderItem

Represents an individual product line within a purchase order.

**Properties**
- `Id` — Primary key.
- `PurchaseOrderId` — Foreign key referencing PurchaseOrder.
- `ProductId` — Foreign key referencing Product.
- `QuantityOrdered` — Number of units ordered.
- `QuantityReceived` — Number of units received.
- `UnitCost` — Cost per unit recorded for this order.

**Relationships**
- Each PurchaseOrderItem belongs to one PurchaseOrder.
- Each PurchaseOrderItem references one Product.

**Business Rules**
- QuantityOrdered must be greater than zero.
- QuantityReceived cannot be negative or exceed QuantityOrdered.
- UnitCost cannot be negative.
- A purchase order cannot contain duplicate lines for the same product in the current MVP.
- A purchase order must contain at least one valid item before it can be submitted.

### 2.6 StockMovement

Represents a record of a change in inventory quantity.

**Properties**
- `Id` — Primary key.
- `ProductId` — Foreign key referencing Product.
- `PurchaseOrderId` — Optional foreign key referencing PurchaseOrder.
- `MovementType` — Type of stock movement.
- `QuantityChange` — Signed quantity change.
- `QuantityBefore` — Stock quantity before the movement.
- `QuantityAfter` — Stock quantity after the movement.
- `Reason` — Explanation for the movement.
- `CreatedAt` — Date and time the movement occurred.

**Relationships**
- Each StockMovement belongs to one Product.
- A StockMovement may reference a PurchaseOrder.

**Movement Types**
- `StockIn` — Inventory increases.
- `StockOut` — Inventory decreases.
- `Adjustment` — Inventory is corrected to reflect an actual count or other justified change.

**Business Rules**
- QuantityChange cannot be zero.
- QuantityBefore and QuantityAfter cannot be negative.
- QuantityAfter must equal QuantityBefore plus QuantityChange.
- Stock movements are audit records and should not be edited or deleted through ordinary application operations.

## 3. Entity Relationships

| Relationship | Cardinality |
|---|---|
| Category → Product | One-to-many |
| Supplier → PurchaseOrder | One-to-many |
| PurchaseOrder → PurchaseOrderItem | One-to-many |
| Product → PurchaseOrderItem | One-to-many |
| Product → StockMovement | One-to-many |
| PurchaseOrder → StockMovement | One-to-many, with an optional purchase-order reference on each movement |

PurchaseOrderItem connects PurchaseOrder and Product while storing order-specific quantities and unit cost.

## 4. Data Integrity

The database and application must work together to maintain valid data.

- Primary keys uniquely identify records.
- Foreign keys maintain valid relationships between entities.
- SKU and OrderNumber must be unique.
- SKU uniqueness must be case-insensitive.
- Stock quantities and prices must follow the validation rules above.
- Purchase-order status transitions must follow the defined workflow.
- Receiving an order must update inventory, create stock movement records, and change the order status within one database transaction.

Application validation should provide clear error messages, while database constraints and indexes should protect critical data integrity.

## 5. Identity and Audit Fields

Authentication and role management are handled separately by the `StockPilot.Identity` project using ASP.NET Core Identity.

Domain entities should not reference the Identity `ApplicationUser` class through navigation properties.

Entities that inherit from `AuditableEntity` use the audit fields defined in that class. The actual entity implementations and EF Core configurations remain the source of truth for which fields are present.

## 6. Scope Limitations

The current MVP does not include:

- Multiple warehouses.
- Barcode scanning.
- Checkout or sales processing.
- Payment gateways.
- Accounting.
- Partial purchase-order receiving.
- AI-based stock forecasting.
- A mobile application.

## 7. Maintenance

Update this document whenever an agreed change modifies the entity model, relationships, or database rules.

Keep the documentation consistent with the actual entity classes and EF Core configurations in the repository.
