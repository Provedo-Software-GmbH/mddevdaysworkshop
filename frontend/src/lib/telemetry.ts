import { trace, SpanStatusCode, type Span, type Tracer } from '@opentelemetry/api';
import { WebTracerProvider, BatchSpanProcessor } from '@opentelemetry/sdk-trace-web';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { resourceFromAttributes } from '@opentelemetry/resources';
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION, ATTR_SERVICE_NAMESPACE } from '@opentelemetry/semantic-conventions';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
import { ZoneContextManager } from '@opentelemetry/context-zone';
import { registerInstrumentations } from '@opentelemetry/instrumentation';

const SERVICE_NAME = 'DevConfTicketing.Frontend';
const SERVICE_NAMESPACE = 'devconf-ticketing';

let tracerInstance: Tracer | null = null;

/**
 * Initialize OpenTelemetry for the frontend.
 * Configures distributed tracing with fetch auto-instrumentation and document load metrics.
 * Traces are exported via OTLP to Application Insights (via the backend's OTLP collector or directly).
 *
 * Call this once in main.tsx before rendering the app.
 */
export function initTelemetry(): void {
  const otlpEndpoint = import.meta.env.VITE_OTLP_ENDPOINT as string | undefined;

  const resource = resourceFromAttributes({
    [ATTR_SERVICE_NAME]: SERVICE_NAME,
    [ATTR_SERVICE_VERSION]: import.meta.env.VITE_APP_VERSION ?? '0.0.0',
    [ATTR_SERVICE_NAMESPACE]: SERVICE_NAMESPACE,
  });

  const spanProcessors = [];

  // Only add the OTLP exporter when an endpoint is configured (production / staging).
  // In local development traces are still created and visible via browser devtools
  // or a local OpenTelemetry Collector if one is running.
  if (otlpEndpoint) {
    const exporter = new OTLPTraceExporter({
      url: `${otlpEndpoint}/v1/traces`,
    });
    spanProcessors.push(new BatchSpanProcessor(exporter));
  }

  const provider = new WebTracerProvider({
    resource,
    spanProcessors,
  });

  provider.register({
    contextManager: new ZoneContextManager(),
  });

  // Auto-instrument all fetch() calls — propagates trace context to the backend API
  // and records spans for every HTTP request.
  registerInstrumentations({
    instrumentations: [
      new FetchInstrumentation({
        // Only propagate trace headers to our own API to avoid CORS issues with third-party APIs
        propagateTraceHeaderCorsUrls: [
          new RegExp(`${window.location.origin}/api/.*`),
          new RegExp(import.meta.env.VITE_API_URL ?? '/api'),
        ],
        clearTimingResources: true,
      }),
      new DocumentLoadInstrumentation(),
    ],
  });

  tracerInstance = trace.getTracer(SERVICE_NAME);
}

/** Returns the frontend tracer. Must be called after {@link initTelemetry}. */
export function getTracer(): Tracer {
  if (!tracerInstance) {
    // Fallback: return a noop tracer so callers never have to null-check
    return trace.getTracer(SERVICE_NAME);
  }
  return tracerInstance;
}

/**
 * Start a new span for a frontend operation (e.g. page navigation, form submit, user action).
 * Returns the span so the caller can add attributes, record events, and end it.
 *
 * @example
 * ```ts
 * const span = startSpan('page.eventDetail', { 'event.id': eventId });
 * try {
 *   // ... perform work
 * } finally {
 *   span.end();
 * }
 * ```
 */
export function startSpan(name: string, attributes?: Record<string, string | number | boolean>): Span {
  const tracer = getTracer();
  const span = tracer.startSpan(name);
  if (attributes) {
    for (const [key, value] of Object.entries(attributes)) {
      span.setAttribute(key, value);
    }
  }
  return span;
}

/**
 * Track a page view as a span. Call this on route changes.
 */
export function trackPageView(routeName: string, path: string): void {
  const span = startSpan('page.view', {
    'page.route': routeName,
    'page.path': path,
    'page.url': window.location.href,
    'page.title': document.title,
  });
  span.end();
}

/**
 * Record an error on the current span and set the span status to ERROR.
 */
export function trackError(span: Span, error: unknown): void {
  const message = error instanceof Error ? error.message : String(error);
  span.setStatus({ code: SpanStatusCode.ERROR, message });
  span.recordException(error instanceof Error ? error : new Error(message));
}

/**
 * Convenience: wrap an async operation in a span. The span is automatically ended
 * and errors are tracked.
 *
 * @example
 * ```ts
 * const events = await withSpan('loadEvents', async (span) => {
 *   span.setAttribute('filter.status', 'published');
 *   return api.get<Event[]>('/events');
 * });
 * ```
 */
export async function withSpan<T>(
  name: string,
  fn: (span: Span) => Promise<T>,
  attributes?: Record<string, string | number | boolean>,
): Promise<T> {
  const span = startSpan(name, attributes);
  try {
    const result = await fn(span);
    span.setStatus({ code: SpanStatusCode.OK });
    return result;
  } catch (error) {
    trackError(span, error);
    throw error;
  } finally {
    span.end();
  }
}
