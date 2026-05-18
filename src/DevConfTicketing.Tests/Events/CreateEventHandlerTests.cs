using System.Diagnostics;
using DevConfTicketing.Application.Events;
using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Events;

namespace DevConfTicketing.Tests.Events;

public class CreateEventHandlerTests
{
    private readonly FakeTelemetryService _telemetry = new();

    [Fact]
    public async Task HandleAsync_ValidEvent_CreatesWithDraftStatus()
    {
        var repository = new FakeEventRepository();
        var handler = new CreateEventHandler(repository, _telemetry);

        var input = CreateValidEvent();
        var result = await handler.HandleAsync(input);

        Assert.Equal(EventStatus.Draft, result.Status);
        Assert.NotEqual(input.Id, result.Id); // Should generate new ID
        Assert.Equal(input.Title, result.Title);
        Assert.Equal(input.Description, result.Description);
        Assert.Equal(input.Location, result.Location);
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_SetsTimestamps()
    {
        var repository = new FakeEventRepository();
        var handler = new CreateEventHandler(repository, _telemetry);

        var before = DateTimeOffset.UtcNow;
        var result = await handler.HandleAsync(CreateValidEvent());
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(result.CreatedAt, before, after);
        Assert.InRange(result.UpdatedAt, before, after);
        Assert.Equal(result.CreatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_PersistsToRepository()
    {
        var repository = new FakeEventRepository();
        var handler = new CreateEventHandler(repository, _telemetry);

        var result = await handler.HandleAsync(CreateValidEvent());

        var stored = await repository.GetByIdAsync(result.Id);
        Assert.NotNull(stored);
        Assert.Equal(result.Title, stored.Title);
    }

    [Fact]
    public async Task HandleAsync_ValidEvent_GeneratesUniqueIds()
    {
        var repository = new FakeEventRepository();
        var handler = new CreateEventHandler(repository, _telemetry);

        var first = await handler.HandleAsync(CreateValidEvent());
        var second = await handler.HandleAsync(CreateValidEvent());

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task HandleAsync_PreservesAllInputFields()
    {
        var repository = new FakeEventRepository();
        var handler = new CreateEventHandler(repository, _telemetry);

        var input = new Event
        {
            Id = "ignored",
            Title = "DevDays Munich 2026",
            Description = "The biggest developer conference in Munich",
            Location = "Alte Kongresshalle, Munich",
            StartDate = new DateTimeOffset(2026, 9, 15, 9, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 9, 17, 18, 0, 0, TimeSpan.Zero),
            OrganizerId = "org-456",
            MaxAttendees = 500,
            ImageUrl = new Uri("https://example.com/image.jpg"),
            WebsiteUrl = new Uri("https://devdays.munich.example.com")
        };

        var result = await handler.HandleAsync(input);

        Assert.Equal("DevDays Munich 2026", result.Title);
        Assert.Equal("The biggest developer conference in Munich", result.Description);
        Assert.Equal("Alte Kongresshalle, Munich", result.Location);
        Assert.Equal(new DateTimeOffset(2026, 9, 15, 9, 0, 0, TimeSpan.Zero), result.StartDate);
        Assert.Equal(new DateTimeOffset(2026, 9, 17, 18, 0, 0, TimeSpan.Zero), result.EndDate);
        Assert.Equal("org-456", result.OrganizerId);
        Assert.Equal(500, result.MaxAttendees);
        Assert.Equal(new Uri("https://example.com/image.jpg"), result.ImageUrl);
        Assert.Equal(new Uri("https://devdays.munich.example.com"), result.WebsiteUrl);
    }

    private static Event CreateValidEvent() => new()
    {
        Id = "temp-id",
        Title = "Test Conference",
        Description = "A test conference description",
        Location = "Berlin, Germany",
        StartDate = DateTimeOffset.UtcNow.AddDays(30),
        EndDate = DateTimeOffset.UtcNow.AddDays(32),
        OrganizerId = "organizer-1",
        MaxAttendees = 200
    };
}

internal sealed class FakeEventRepository : IEventRepository
{
    private readonly Dictionary<string, Event> _events = [];

    public Task<Event?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_events.GetValueOrDefault(id));

    public Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Event>>(_events.Values.ToList());

    public Task<IReadOnlyList<Event>> GetByStatusAsync(EventStatus status, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Event>>(_events.Values.Where(e => e.Status == status).ToList());

    public Task<Event> CreateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        _events[@event.Id] = @event;
        return Task.FromResult(@event);
    }

    public Task<Event> UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        _events[@event.Id] = @event;
        return Task.FromResult(@event);
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _events.Remove(id);
        return Task.CompletedTask;
    }
}

internal sealed class FakeTelemetryService : ITelemetryService
{
    public Activity? StartSpan(string operationName, ActivityKind kind = ActivityKind.Internal, IDictionary<string, string>? tags = null) => null;
    public void TrackEvent(string eventName, IDictionary<string, string>? properties = null) { }
    public void TrackException(Exception exception, IDictionary<string, string>? properties = null) { }
    public void RecordHistogram(string name, double value, IDictionary<string, string>? tags = null) { }
    public void IncrementCounter(string name, long delta = 1, IDictionary<string, string>? tags = null) { }
}
