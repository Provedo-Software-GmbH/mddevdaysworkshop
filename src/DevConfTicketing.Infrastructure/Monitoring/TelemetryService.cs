using System.Diagnostics;
using System.Diagnostics.Metrics;

using DevConfTicketing.Application.Interfaces;

using Microsoft.ApplicationInsights;

namespace DevConfTicketing.Infrastructure.Monitoring;

public class TelemetryService : ITelemetryService
{
    private static readonly ActivitySource ActivitySource = new("DevConfTicketing");
    private static readonly Meter Meter = new("DevConfTicketing");

    private readonly TelemetryClient _telemetryClient;
    private readonly Dictionary<string, Histogram<double>> _histograms = [];

    public TelemetryService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null) =>
        _telemetryClient.TrackEvent(eventName, properties, metrics);

    public void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
    {
        _telemetryClient.GetMetric(name).TrackValue(value);

        if (properties is not null)
        {
            _telemetryClient.TrackEvent($"Metric:{name}", properties, new Dictionary<string, double> { [name] = value });
        }
    }

    public void TrackException(Exception exception, IDictionary<string, string>? properties = null) =>
        _telemetryClient.TrackException(exception, properties);

    public Activity? StartSpan(string operationName, ActivityKind kind = ActivityKind.Internal, IDictionary<string, string>? tags = null)
    {
        var activity = ActivitySource.StartActivity(operationName, kind);

        if (activity is not null && tags is not null)
        {
            foreach (var tag in tags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }
        }

        return activity;
    }

    public void RecordHistogram(string name, double value, IDictionary<string, string>? tags = null)
    {
        if (!_histograms.TryGetValue(name, out var histogram))
        {
            histogram = Meter.CreateHistogram<double>(name);
            _histograms[name] = histogram;
        }

        if (tags is not null)
        {
            var tagList = new TagList();
            foreach (var tag in tags)
            {
                tagList.Add(tag.Key, tag.Value);
            }
            histogram.Record(value, tagList);
        }
        else
        {
            histogram.Record(value);
        }
    }
}
