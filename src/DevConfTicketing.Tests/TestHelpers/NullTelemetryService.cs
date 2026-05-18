using System.Diagnostics;

using DevConfTicketing.Application.Interfaces;

namespace DevConfTicketing.Tests.TestHelpers;

/// <summary>
/// No-op telemetry service for unit tests.
/// </summary>
public class NullTelemetryService : ITelemetryService
{
    public Activity? StartSpan(string operationName, ActivityKind kind = ActivityKind.Internal, IDictionary<string, string>? tags = null) => null;
    public void TrackEvent(string eventName, IDictionary<string, string>? properties = null) { }
    public void TrackException(Exception exception, IDictionary<string, string>? properties = null) { }
    public void RecordHistogram(string name, double value, IDictionary<string, string>? tags = null) { }
    public void IncrementCounter(string name, long delta = 1, IDictionary<string, string>? tags = null) { }
}
