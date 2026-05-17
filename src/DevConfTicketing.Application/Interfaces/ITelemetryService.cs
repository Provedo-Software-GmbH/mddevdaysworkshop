using System.Diagnostics;

namespace DevConfTicketing.Application.Interfaces;

public interface ITelemetryService
{
    Activity? StartSpan(string operationName, ActivityKind kind = ActivityKind.Internal, IDictionary<string, string>? tags = null);
    void TrackEvent(string eventName, IDictionary<string, string>? properties = null);
    void TrackException(Exception exception, IDictionary<string, string>? properties = null);
    void RecordHistogram(string name, double value, IDictionary<string, string>? tags = null);
    void IncrementCounter(string name, long delta = 1, IDictionary<string, string>? tags = null);
}
