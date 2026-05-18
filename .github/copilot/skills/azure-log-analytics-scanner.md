# Azure Log Analytics Scanner

## Description

Scans Azure Log Analytics workspace and Application Insights for errors and extracts them for agents to fix. Provides KQL query templates and guidance on interpreting telemetry data from the DevConfTicketing application.

## Querying for Recent Exceptions

```kql
// Recent exceptions in the last 24 hours
exceptions
| where timestamp > ago(24h)
| summarize count() by type, outerMessage, innermostMessage
| order by count_ desc
| take 20
```

## Querying for Error Traces by Endpoint

```kql
// Failed requests by endpoint
requests
| where timestamp > ago(24h)
| where success == false
| summarize count() by name, resultCode
| order by count_ desc
```

## Querying for Slow Requests

```kql
// Requests slower than 2 seconds
requests
| where timestamp > ago(24h)
| where duration > 2000
| project timestamp, name, duration, resultCode, customDimensions
| order by duration desc
| take 50
```

## Querying Custom Metrics

```kql
// Business event counters (e.g., tickets sold, orders created)
customMetrics
| where timestamp > ago(24h)
| where name startswith "DevConfTicketing"
| summarize sum(value) by name, bin(timestamp, 1h)
| render timechart
```

## Querying Cosmos DB Performance

```kql
// Cosmos DB operation durations
customMetrics
| where timestamp > ago(24h)
| where name == "db.cosmos.duration"
| extend operation = tostring(customDimensions["db.operation"])
| extend container = tostring(customDimensions["db.cosmos.container"])
| summarize avg(value), percentile(value, 95), percentile(value, 99) by operation, container
```

## Querying Distributed Traces

```kql
// End-to-end trace for a specific correlation ID
union requests, dependencies, traces, exceptions
| where operation_Id == "<correlation-id>"
| order by timestamp asc
| project timestamp, itemType, name, message, duration, resultCode
```

## Correlating Frontend and Backend Traces

The frontend sends traces via OTLP/HTTP JSON to `POST /api/v1/telemetry`, which are re-exported through the backend's Azure Monitor pipeline. Frontend spans include:
- `browser.page_view` — page navigation events
- `browser.interaction` — user interactions
- `http.request` — API calls from the frontend

```kql
// Frontend page views correlated with backend requests
let frontendTraces = traces
| where message startswith "browser."
| project operation_Id, timestamp, frontendEvent = message;
let backendRequests = requests
| project operation_Id, timestamp, backendEndpoint = name, duration;
frontendTraces
| join kind=inner backendRequests on operation_Id
| project frontendEvent, backendEndpoint, duration
```

## Interpreting Exception Telemetry

When analyzing exceptions from Application Insights:

1. **Check `outerType`** — the .NET exception type (e.g., `System.InvalidOperationException`)
2. **Check `outerMessage`** — the exception message with context
3. **Check `innermostType` and `innermostMessage`** — the root cause
4. **Check `customDimensions`** — contains:
   - `operationName` — the API endpoint or handler that threw
   - `eventId`, `orderId` etc. — business context from span tags
5. **Check `operation_Id`** — use to trace the full request flow

## Mapping Exceptions to Source Code

The DevConfTicketing application structure:
- API endpoints: `src/DevConfTicketing.Api/Endpoints/*.cs`
- Business logic handlers: `src/DevConfTicketing.Application/{Feature}/*.cs`
- Repository operations: `src/DevConfTicketing.Infrastructure/Cosmos/Repositories/*.cs`
- Telemetry: `src/DevConfTicketing.Infrastructure/Monitoring/TelemetryService.cs`

Exception patterns:
- `ArgumentException` in handlers → input validation failure in Application layer
- `InvalidOperationException` in handlers → business rule violation in Application layer
- `CosmosException` in repositories → database operation failure in Infrastructure layer
- `HttpRequestException` → external service call failure (e.g., Stripe)

## Output Format for Fix Tasks

When reporting errors, format as:

```
### Error: {Exception Type}
- **Severity**: Critical / High / Medium / Low
- **Frequency**: {count} occurrences in last 24h
- **Affected Endpoint**: {endpoint}
- **Error Message**: {message}
- **Root Cause**: {innermostMessage}
- **Affected Users**: ~{estimate}
- **Source File**: {likely source file path}
- **Suggested Fix**: {actionable description}
```

## Alerting Thresholds

- **Error rate > 5%** of requests → Critical
- **P99 latency > 2s** for any endpoint → High
- **Cosmos DB RU > 100** for a single operation → Medium
- **Exception count > 10/hour** for same type → High
- **Checkout failure rate > 2%** → Critical
