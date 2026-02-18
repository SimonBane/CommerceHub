# CommerceHub Resilience and Idempotency

This document describes the resilience patterns and idempotency mechanisms applied across CommerceHub services.

## Resilience Patterns

### HTTP Client Resilience (BuildingBlocks)

All services using `AddServiceDefaults()` get `AddStandardResilienceHandler()` on HTTP clients, which provides:

- **Retry** with exponential backoff for transient failures
- **Circuit breaker** to avoid cascading failures
- **Timeout** to prevent long-running requests from blocking

### Wolverine Message Handler Resilience

`UseResiliencePolicies()` (from `CommerceHub.BuildingBlocks`) applies an `IHandlerPolicy` to all Wolverine message handlers:

| Exception Type | Action |
|----------------|--------|
| `NpgsqlException` | Retry with cooldown (50ms, 100ms, 250ms, 500ms) |
| `DbException` | Retry with cooldown (50ms, 100ms, 250ms) |
| `SocketException` | Schedule retry after 5 seconds |
| `HttpRequestException` | Schedule retry after 5 seconds |
| `TimeoutException` | Schedule retry after 5 seconds |
| `InvalidOperationException` (message contains "already exists") | Discard (e.g. duplicate order) |

### Circuit Breaker (CheckoutOrchestrator)

RabbitMQ listeners in CheckoutOrchestrator use a per-endpoint circuit breaker:

- **MinimumThreshold**: 5 messages before evaluation
- **PauseTime**: 1 minute when tripped
- **FailurePercentageThreshold**: 25%

When the failure rate exceeds the threshold, message processing is paused to allow downstream services to recover.

## Idempotency

### Message Handlers (Already Idempotent)

- **ReserveStockHandler** (InventoryService): Checks for existing reservation by `OrderId` before creating
- **InitiatePaymentHandler** (PaymentService): Returns existing payment result when payment for `OrderId` already exists
- **CreateOrderHandler** (OrderingService): Event-sourced; throws if order already exists (handled by discard policy)
- **SearchService projections**: Version-based idempotency (`event.Version >= doc.Version`)

### HTTP Idempotency (InitiateCheckout)

`POST /basket/checkout` supports the `Idempotency-Key` header:

- When the client sends `Idempotency-Key: <unique-key>`, the response is cached in Redis (FusionCache) for 24 hours
- Duplicate requests with the same key return the cached response without re-publishing `CheckoutInitiatedV1`
- Clients should generate a unique key per logical checkout attempt (e.g. UUID v7)

## Usage

### Applying Resilience to a New Wolverine Service

```csharp
builder.Host.UseWolverine(opts =>
{
    opts.Policies.UseDurableLocalQueues();
    opts.UseResiliencePolicies();  // from CommerceHub.BuildingBlocks
    // ... transport and handler configuration
});
```

### Adding Circuit Breaker to a RabbitMQ Listener

```csharp
opts.ListenToRabbitQueue("my-queue")
    .CircuitBreaker(cb =>
    {
        cb.MinimumThreshold = 5;
        cb.PauseTime = TimeSpan.FromMinutes(1);
        cb.FailurePercentageThreshold = 25;
    });
```
