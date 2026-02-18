# CommerceHub Observability Stack

CommerceHub services emit OpenTelemetry tracing, metrics, and logs. This document describes the instrumentations in place and how to view telemetry locally or in production.

## Instrumentation Overview

All services using `AddServiceDefaults()` (from `CommerceHub.BuildingBlocks`) are configured with:

### Logging
- Structured OpenTelemetry logging with formatted messages and scopes
- Exported via OTLP when `OTEL_EXPORTER_OTLP_ENDPOINT` is set

### Metrics
- **ASP.NET Core**: HTTP request duration, count
- **HttpClient**: Outbound HTTP call duration, count
- **Runtime**: .NET garbage collection, JIT, thread pool
- **Process**: CPU time, memory usage, thread count (per service)
- **Npgsql**: PostgreSQL connection and command metrics
- **Wolverine**: Message handling metrics

### Tracing
- **ASP.NET Core**: HTTP request spans (health/alive excluded)
- **HttpClient**: Outbound HTTP spans
- **Npgsql**: PostgreSQL command spans
- **Redis**: Redis command spans (BasketService when `IConnectionMultiplexer` is registered)
- **Wolverine**: Message handler spans
- **MongoDB**: MongoDB operation spans (SearchService, BackofficeApi)

## Local Development with Aspire

When running via the CommerceHub AppHost, Aspire's built-in dashboard receives traces and metrics. Set:

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317  # or Aspire OTLP endpoint
```

If your Aspire dashboard is configured with OTLP, telemetry will appear there automatically. Check the Aspire dashboard URL (typically `http://localhost:15xxx`) for distributed traces and metrics.

## Production Setup

### 1. OpenTelemetry Collector (recommended)

Run an [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/) that receives OTLP and fans out to your backends:

```yaml
# otel-collector-config.yaml (example)
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

exporters:
  prometheus:
    endpoint: "0.0.0.0:9464"
  otlp:
    endpoint: "tempo:4317"  # or your trace backend

processors:
  batch:
    timeout: 1s

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [prometheus]
```

### 2. Environment Variables

Set on each CommerceHub service:

| Variable | Description |
|----------|-------------|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OTLP gRPC endpoint (e.g. `http://otel-collector:4317`) |
| `OTEL_SERVICE_NAME` | Override service name (default: application name) |

### 3. Grafana

- **Metrics**: Configure Prometheus (or Mimir) as a data source scraping the OpenTelemetry Collector Prometheus exporter.
- **Traces**: Configure Tempo (or Jaeger) as a data source receiving OTLP from the collector.

Sample dashboards are in this folder. Import them via Grafana UI: **Dashboards → Import → Upload JSON**.

### 4. Jaeger (optional)

To use Jaeger for traces, configure the OpenTelemetry Collector to export to Jaeger's OTLP receiver, or run Jaeger with OTLP enabled.

## Sample Dashboards

- `commercehub-services-dashboard.json` – Request rates, latencies, and process metrics by service
- `commercehub-process-dashboard.json` – Process CPU, memory, and thread counts (from `OpenTelemetry.Instrumentation.Process`)

## Architecture Plan

Observability is tracked as **observability-stack** in `docs/CommerceHub-architecture-plan.md`. This folder completes the "sample Grafana/Jaeger dashboards" requirement.
