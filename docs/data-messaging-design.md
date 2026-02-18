# CommerceHub Data & Messaging Design

This document finalizes message contracts, database schemas per bounded context, and eventual consistency strategies.

---

## 1. Message Contracts

All shared contracts live in `CommerceHub.Contracts`. Events and commands use a versioning suffix (`V1`) for backward compatibility.

### 1.1 Events

| Event | Publisher | Consumer(s) | Description |
|-------|-----------|-------------|-------------|
| `CheckoutInitiatedV1` | BasketService | CheckoutOrchestrator | Basket checkout started; starts the checkout saga |
| `ProductCreated`, `ProductUpdated`, `ProductDeleted` | CatalogService | SearchService | Catalog changes for search index projection |
| `CategoryCreated`, `CategoryUpdated`, `CategoryDeleted` | CatalogService | SearchService | Category changes for search index |
| `InventoryReservedV1` | InventoryService | CheckoutOrchestrator | Stock reserved; continue to payment |
| `InventoryReservationFailedV1` | InventoryService | CheckoutOrchestrator | Reservation failed; fail saga |
| `PaymentAuthorizedV1` | PaymentService | CheckoutOrchestrator | Payment OK; create order |
| `PaymentFailedV1` | PaymentService | CheckoutOrchestrator | Payment failed; release reservation |
| `OrderPlacedV1` | OrderingService | CheckoutOrchestrator, NotificationService | Order created; complete saga, notify |
| `OrderPaidV1` | OrderingService | NotificationService | Order paid notification |
| `OrderCancelledV1` | OrderingService | NotificationService | Order cancelled notification |
| `OrderShippedV1` | OrderingService | NotificationService | Order shipped notification |

### 1.2 Commands

| Command | Sender | Handler | Description |
|---------|-------|---------|-------------|
| `ReserveInventoryCommand` | CheckoutOrchestrator | InventoryService | Reserve stock for order |
| `ReleaseInventoryCommand` | CheckoutOrchestrator | InventoryService | Release reservation on failure |
| `InitiatePaymentCommand` | CheckoutOrchestrator | PaymentService | Authorize payment |
| `CreateOrderCommand` | CheckoutOrchestrator | OrderingService | Create event-sourced order |

### 1.3 Queue Routing

| Queue | Producers | Consumers |
|-------|-----------|-----------|
| `basket-events` | BasketService | CheckoutOrchestrator |
| `catalog-events` | CatalogService | SearchService |
| `inventory-commands` | CheckoutOrchestrator | InventoryService |
| `inventory-events` | InventoryService | CheckoutOrchestrator |
| `payment-commands` | CheckoutOrchestrator | PaymentService |
| `payment-events` | PaymentService | CheckoutOrchestrator |
| `order-commands` | CheckoutOrchestrator | OrderingService |
| `order-events` | OrderingService | CheckoutOrchestrator, NotificationService |

### 1.4 Versioning

- Catalog events include a `Version` (timestamp) for idempotent projections.
- Event names use `V1` suffix; future breaking changes use `V2`, etc.
- Commands are not versioned; backward compatibility is maintained by adding optional fields.

---

## 2. Database Schemas per Bounded Context

CommerceHub uses a shared PostgreSQL server with one database per bounded context. MongoDB is used for read models and projections.

### 2.1 PostgreSQL Databases (Aspire Connection Names)

| Database | Service | Purpose |
|----------|---------|---------|
| `identityDb` | IdentityService | ASP.NET Core Identity (users, roles, claims) |
| `catalogDb` | CatalogService | Products, categories (EF Core) |
| `inventoryDb` | InventoryService | SkuStock, Reservations, ReservationItems (EF Core) |
| `paymentDb` | PaymentService | Payments (EF Core) |
| `orderingDb` | OrderingService | Marten event store + snapshots for Order aggregate |
| `checkoutOrchestratorDb` | CheckoutOrchestrator | Wolverine message persistence, saga state (CheckoutSaga) |
| `notificationDb` | NotificationService | Marten outbox, notification state |

### 2.2 EF Core Schemas (Catalog, Inventory, Payment, Identity)

**Catalog (catalogDb)**
- `Products`: Id, Name, Description, Sku (unique), CategoryId, BasePrice, ImageUrl, CreatedAt, UpdatedAt
- `Categories`: Id, Name, Slug (unique), ParentCategoryId

**Inventory (inventoryDb)**
- `SkuStocks`: Id, ProductId, Sku, TotalQuantity, ReservedQuantity, AvailableQuantity
- `Reservations`: Id, OrderId, CustomerId, Status, CreatedAt
- `ReservationItems`: Id, ReservationId, ProductId, Sku, Quantity

**Payment (paymentDb)**
- `Payments`: Id, OrderId, CustomerId, Amount, CurrencyCode, Status, ExternalTransactionId, AuthorizedAt, FailedAt, FailureReason

**Identity (identityDb)**
- ASP.NET Core Identity tables: AspNetUsers, AspNetRoles, AspNetUserRoles, etc.

### 2.3 Marten (Event Sourcing)

**Ordering (orderingDb)**
- Event stream: `OrderPlaced`, `OrderPaid`, `OrderCancelled`, `OrderShipped`
- Aggregate: `Order` (Id, CustomerId, Lines, Total, Status, etc.)
- Snapshots: Inline snapshots for `Order`

**CheckoutOrchestrator (checkoutOrchestratorDb)**
- Saga state: `CheckoutSaga` (OrderId, CustomerId, Items, Step, ReservationId, etc.)
- Wolverine durable outbox for commands

**Notification (notificationDb)**
- Marten outbox for notification events

### 2.4 MongoDB Collections

| Database | Collection | Service | Purpose |
|----------|------------|---------|---------|
| `commercehub_search` | `product_search` | SearchService | Product search index (from catalog events) |
| `commercehub_search` | (various) | BackofficeApi | Admin read models (products, orders) |

### 2.5 Redis

| Key pattern | Service | Purpose |
|-------------|---------|---------|
| `basket:{customerId}` | BasketService | Shopping basket (FusionCache) |
| `checkout-idempotency:{key}` | BasketService | Idempotency cache for checkout |

---

## 3. Eventual Consistency Strategies

### 3.1 Catalog → Search

- **Flow**: CatalogService publishes `ProductCreated`/`Updated`/`Deleted` and `Category*` to `catalog-events`. SearchService consumes and projects into MongoDB `product_search`.
- **Idempotency**: Events carry `Version` (timestamp). MongoDB documents have `Version`. Handler applies only when `event.Version >= doc.Version`. Replays and out-of-order delivery do not overwrite newer data.
- **Latency**: Typically seconds. Search is eventually consistent; clients may not see new products immediately.

### 3.2 Checkout Saga (Transactional Consistency Boundary)

- **Flow**: CheckoutOrchestrator coordinates ReserveInventory → InitiatePayment → CreateOrder. Each step publishes an event; the saga consumes events and sends the next command.
- **Consistency**: Saga is a process manager. Each service (Inventory, Payment, Ordering) is authoritative for its own data. Saga ensures all-or-nothing semantics via compensating actions (ReleaseInventory on PaymentFailed).
- **Idempotency**: InventoryService checks existing reservation by OrderId. PaymentService checks existing payment by OrderId. OrderingService (event-sourced) rejects duplicate CreateOrder for same OrderId.

### 3.3 Ordering → Backoffice / Notifications

- **Flow**: OrderingService publishes `OrderPlacedV1`, `OrderPaidV1`, `OrderCancelledV1`, `OrderShippedV1` to `order-events`. NotificationService and BackofficeApi consume.
- **Read-your-writes**: Backoffice reads from OrderingService HTTP API (direct) and MongoDB projections (eventual). For immediate reads after create, use direct API. For lists/history, MongoDB is eventually consistent.

### 3.4 Outbox Pattern

- **CatalogService**: EF Core + Wolverine `IDbContextOutbox`. Entity changes and `PublishAsync` are committed atomically via `SaveChangesAndFlushMessagesAsync()`.
- **InventoryService, PaymentService**: Same pattern with `IDbContextOutbox`.
- **OrderingService**: Marten event store + Wolverine; events and outbound messages are transactional.
- **NotificationService**: Marten outbox for reliable notification delivery.

### 3.5 Handling Failures

- **Retries**: Wolverine retry policies (exponential backoff) for transient DB/network errors.
- **Circuit breaker**: CheckoutOrchestrator RabbitMQ listeners use circuit breaker to pause on high failure rates.
- **Dead letter**: Messages that exhaust retries move to error queue for manual inspection.

---

## 4. Connection String Reference

For local development (Aspire), connection strings are injected by AppHost. For standalone/Docker:

| Key | Example |
|-----|---------|
| `identityDb` | `Host=localhost;Port=5432;Database=commercehub_identity;...` |
| `catalogDb` | `Host=localhost;Port=5432;Database=commercehub_catalog;...` |
| `inventoryDb` | `Host=localhost;Port=5432;Database=commercehub_inventory;...` |
| `paymentDb` | `Host=localhost;Port=5432;Database=commercehub_payment;...` |
| `orderingDb` | `Host=localhost;Port=5432;Database=commercehub_ordering;...` |
| `checkoutOrchestratorDb` | `Host=localhost;Port=5432;Database=commercehub_checkout;...` |
| `notificationDb` | `Host=localhost;Port=5432;Database=commercehub_notification;...` |
| `mongoDb` | `mongodb://localhost:27017` |
| `redis` | `localhost:6379` |
| `rabbitMq` | `amqp://localhost` |
