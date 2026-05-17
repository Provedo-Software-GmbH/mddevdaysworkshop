using System.Diagnostics;
using System.Diagnostics.Metrics;

using DevConfTicketing.Application.Interfaces;

namespace DevConfTicketing.Infrastructure.Monitoring;

public class TelemetryService : ITelemetryService
{
    private static readonly ActivitySource ActivitySource = new("DevConfTicketing");
    private static readonly Meter Meter = new("DevConfTicketing");

    private readonly Dictionary<string, Histogram<double>> _histograms = [];
    private readonly Dictionary<string, Counter<long>> _counters = [];

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

    public void TrackEvent(string eventName, IDictionary<string, string>? properties = null)
    {
        var activity = Activity.Current;

        if (activity is not null)
        {
            var activityTags = properties is not null
                ? new ActivityTagsCollection(properties.Select(p => new KeyValuePair<string, object?>(p.Key, p.Value)))
                : null;

            activity.AddEvent(new ActivityEvent(eventName, tags: activityTags));
        }
    }

    public void TrackException(Exception exception, IDictionary<string, string>? properties = null)
    {
        var activity = Activity.Current;

        if (activity is not null)
        {
            var tags = new ActivityTagsCollection
            {
                ["exception.type"] = exception.GetType().FullName,
                ["exception.message"] = exception.Message,
                ["exception.stacktrace"] = exception.StackTrace
            };

            if (properties is not null)
            {
                foreach (var prop in properties)
                {
                    tags[prop.Key] = prop.Value;
                }
            }

            activity.AddEvent(new ActivityEvent("exception", tags: tags));
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        }
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

    public void IncrementCounter(string name, long delta = 1, IDictionary<string, string>? tags = null)
    {
        if (!_counters.TryGetValue(name, out var counter))
        {
            counter = Meter.CreateCounter<long>(name);
            _counters[name] = counter;
        }

        if (tags is not null)
        {
            var tagList = new TagList();
            foreach (var tag in tags)
            {
                tagList.Add(tag.Key, tag.Value);
            }
            counter.Add(delta, tagList);
        }
        else
        {
            counter.Add(delta);
        }
    }
}
