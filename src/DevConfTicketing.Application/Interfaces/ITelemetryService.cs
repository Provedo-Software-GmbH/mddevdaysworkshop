using System.Diagnostics;

namespace DevConfTicketing.Application.Interfaces;

public interface ITelemetryService
{
    void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);
    void TrackMetric(string name, double value, IDictionary<string, string>? properties = null);
    void TrackException(Exception exception, IDictionary<string, string>? properties = null);
    Activity? StartSpan(string operationName, ActivityKind kind = ActivityKind.Internal, IDictionary<string, string>? tags = null);
    void RecordHistogram(string name, double value, IDictionary<string, string>? tags = null);
}
