# StockPilot

Inventory and purchasing management system for a small retail or wholesale business.

## Overview

StockPilot helps a business manage its products, suppliers, stock levels, and purchase orders through a centralized web application.

The goal of this project is to build a full-stack application using Angular and ASP.NET Core Web API, following sound software engineering practices.

## Technology Stack

- **Frontend:** Angular, TypeScript, NgRx
- **Backend:** ASP.NET Core Web API (.NET 10)
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Validation:** FluentValidation
- **Testing:** xUnit
- **Version Control:** Git

## User Roles

### Administrator

- Manage staff accounts and roles.
- Create and manage products and categories.
- Create and manage suppliers.
- Create and submit purchase orders.
- Receive purchased goods.
- Record stock adjustments.
- View dashboard metrics and stock movement history.

### Staff

- View products and stock levels.
- View supplier information.
- Record permitted stock adjustments with a reason.
- View stock movement history.
- Create and manage purchase orders according to the defined workflow.

## MVP Features

### 1. Authentication and Authorization

- User login and logout.
- Administrator and Staff roles.
- Role-based access control.

### 2. Dashboard

- Total inventory value.
- Low-stock product list.
- Pending purchase orders.

### 3. Product and Category Management

- Create, view, update, and deactivate products.
- Create, view, update, and manage categories.
- Assign products to categories.
- Enforce unique product SKUs.

### 4. Supplier Management

- Create, view, update, and deactivate suppliers.
- Associate suppliers with purchase orders.

### 5. Stock Management

- Record stock increases and decreases.
- Require a reason for manual stock adjustments.
- Record stock movement history.
- Prevent inventory quantities from becoming negative.

### 6. Purchase Orders

- Create purchase orders in Draft status.
- Submit valid purchase orders.
- Receive submitted orders.
- Update stock only when goods are received.
- Record stock movements for received goods.

### 7. Data Navigation and Validation

- Search and filter records.
- Support pagination for data tables.
- Validate user input on the frontend and backend.
- Display useful validation and error messages.

## Business Rules

1. Each product must have a unique SKU, ignoring letter case.
2. Product stock quantities cannot be negative.
3. Reorder levels cannot be negative.
4. Stock adjustments must have a nonzero quantity change and a reason.
5. Purchase orders must contain at least one valid item before submission.
6. Purchase order items must have positive ordered quantities and nonnegative unit costs.
7. Purchase orders follow the workflow: Draft → Submitted → Received.
8. Only submitted purchase orders can be received.
9. Submitting a purchase order does not change inventory quantities.
10. Receiving a purchase order updates product quantities and records corresponding stock movements.
11. Inventory updates and stock movement records must be committed in the same database transaction.
12. Stock movement history must be retained and must not be edited to rewrite past inventory activity.
13. Categories, suppliers, products, and users referenced by historical records must not be hard-deleted.
14. Protected operations must enforce authorization on the backend.

## Purchase Order Workflow

1. Create a draft purchase order.
2. Select a supplier.
3. Add products, quantities, and unit costs.
4. Validate and submit the order.
5. Receive the ordered goods.
6. Update inventory quantities and record stock movements.
7. Mark the purchase order as Received.

## Dashboard Definitions

- **Inventory Value:** Sum of each product's quantity on hand multiplied by its cost price.
- **Low-Stock Items:** Active products whose quantity on hand is less than or equal to their reorder level.
- **Pending Purchase Orders:** Purchase orders currently in Submitted status.

## Out of Scope

The initial release will not include:

- Multiple warehouses.
- Barcode scanning.
- Accounting and financial reporting.
- Point-of-sale or checkout functionality.
- Payment gateway integrations.
- AI-powered forecasting.
- Native mobile applications.
- Partial purchase-order receiving.

## Project Goals

- Demonstrate full-stack development with Angular and ASP.NET Core.
- Apply clean architecture and sound domain modeling.
- Implement secure authentication and authorization.
- Use automated tests to verify important business rules.
- Build a responsive, professional user interface.
- Document and deploy a working application.
