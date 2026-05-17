using DevConfTicketing.Application.Interfaces;

using Microsoft.ApplicationInsights;

namespace DevConfTicketing.Infrastructure.Monitoring;

public class TelemetryService(TelemetryClient telemetryClient) : ITelemetryService
{
    public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null) =>
        telemetryClient.TrackEvent(eventName, properties, metrics);

    public void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
    {
        telemetryClient.GetMetric(name).TrackValue(value);

        if (properties is not null)
        {
            telemetryClient.TrackEvent($"Metric:{name}", properties, new Dictionary<string, double> { [name] = value });
        }
    }

    public void TrackException(Exception exception, IDictionary<string, string>? properties = null) =>
        telemetryClient.TrackException(exception, properties);
}
