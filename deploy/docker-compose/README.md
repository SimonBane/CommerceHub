# CommerceHub Docker Compose

Standalone Docker Compose setup that mirrors the Aspire AppHost. Use this when running without Aspire (e.g. production-like local testing, CI).

## Prerequisites

- Docker & Docker Compose
- Run from **repo root** (for build context)

## Quick Start

```bash
# From repo root
docker compose -f deploy/docker-compose/docker-compose.yml up -d

# Or
cd deploy/docker-compose && docker compose up -d
```

## Services

### Infrastructure (matches AppHost)

| Service   | Port(s)  | Description                              |
|----------|----------|------------------------------------------|
| PostgreSQL | 5432   | Databases: identitydb, catalogdb, orderingdb, inventorydb, paymentdb, checkoutorchestratordb, notificationdb |
| Redis    | 6379     | Basket, idempotency                       |
| RabbitMQ | 5672, 15672 | Messaging (AMQP + management UI)    |
| MongoDB  | 27017    | Search index, backoffice projections     |

### Observability

| Service   | Port | Description |
|-----------|------|-------------|
| OpenTelemetry Collector | 4317 (gRPC), 4318 (HTTP), 9464 (Prometheus) | Receives OTLP from services, exports metrics to Prometheus |
| Prometheus | 9090 | Scrapes OTLP collector, stores metrics |
| Grafana | 3000 | Dashboards; Prometheus datasource pre-provisioned |

**Grafana**: http://localhost:3000 (admin / admin)

Pre-provisioned dashboards (from `deploy/observability`):

- **CommerceHub Services** – Request rates, latencies, process metrics by service
- **CommerceHub Process** – Process CPU, memory, thread counts

### CommerceHub APIs

| Service              | Gateway Path | Internal Port |
|----------------------|--------------|---------------|
| Gateway              | `/`          | 5000 → 8080   |
| IdentityService      | `/api/auth/` | 8080          |
| CatalogService       | `/api/catalog/` | 8080       |
| SearchService        | `/api/search/` | 8080       |
| BasketService        | `/api/basket/` | 8080       |
| OrderingService      | `/api/orders/` | 8080       |
| InventoryService     | `/api/inventory/` | 8080   |
| PaymentService       | `/api/payment/` | 8080       |
| BackofficeApi        | `/api/backoffice/` | 8080  |
| CheckoutOrchestrator | (internal)   | 8080          |
| NotificationService  | (internal)   | 8080         |

**Gateway**: http://localhost:5000

## Telemetry

All services export OpenTelemetry metrics to `http://otel-collector:4317`. The collector exports to Prometheus at `:9464`, which Prometheus scrapes. Grafana queries Prometheus.

Set `OTEL_EXPORTER_OTLP_ENDPOINT` when running services outside this stack to point at `http://localhost:4317`.

## First Run

1. Wait for PostgreSQL to be healthy (migrations run on startup).
2. Use the Gateway at http://localhost:5000 for API traffic.
3. Open Grafana at http://localhost:3000 and explore the CommerceHub dashboards.

## Build Only

```bash
docker compose -f deploy/docker-compose/docker-compose.yml build
```

## Stop

```bash
docker compose -f deploy/docker-compose/docker-compose.yml down
```
