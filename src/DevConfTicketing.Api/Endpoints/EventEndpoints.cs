using System.ComponentModel;
using System.Text.Json.Serialization;

using DevConfTicketing.Application.Events;
using DevConfTicketing.Domain.Events;

namespace DevConfTicketing.Api.Endpoints;

public static class EventEndpoints
{
    public static RouteGroupBuilder MapEventEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (GetEventsHandler handler, CancellationToken ct) =>
        {
            var events = await handler.GetAllAsync(ct);
            return Results.Ok(events);
        })
        .WithName("GetEvents")
        .WithTags("Events")
        .WithDescription("Get all events")
        .Produces<IReadOnlyList<Event>>();

        group.MapGet("/{id}", async (string id, GetEventsHandler handler, CancellationToken ct) =>
        {
            var @event = await handler.GetByIdAsync(id, ct);
            return @event is not null ? Results.Ok(@event) : Results.NotFound();
        })
        .WithName("GetEventById")
        .WithTags("Events")
        .WithDescription("Get an event by ID")
        .Produces<Event>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateEventRequest request, CreateEventHandler handler, CancellationToken ct) =>
        {
            var @event = new Event
            {
                Id = string.Empty,
                Title = request.Title,
                Description = request.Description,
                Location = request.Location,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                OrganizerId = request.OrganizerId,
                MaxAttendees = request.MaxAttendees,
                ImageUrl = request.ImageUrl,
                WebsiteUrl = request.WebsiteUrl,
                CreatedAt = default,
                UpdatedAt = default
            };

            var created = await handler.HandleAsync(@event, ct);
            return Results.Created($"/api/v1/events/{created.Id}", created);
        })
        .WithName("CreateEvent")
        .WithTags("Events")
        .WithDescription("Create a new event")
        .Produces<Event>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapPut("/{id}", async (string id, UpdateEventRequest request, UpdateEventHandler handler, CancellationToken ct) =>
        {
            var @event = new Event
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                Location = request.Location,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                OrganizerId = request.OrganizerId,
                MaxAttendees = request.MaxAttendees,
                ImageUrl = request.ImageUrl,
                WebsiteUrl = request.WebsiteUrl,
                CreatedAt = default,
                UpdatedAt = default
            };

            var updated = await handler.HandleAsync(id, @event, ct);
            return Results.Ok(updated);
        })
        .WithName("UpdateEvent")
        .WithTags("Events")
        .WithDescription("Update an existing event")
        .Produces<Event>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id}/publish", async (string id, PublishEventHandler handler, CancellationToken ct) =>
        {
            var published = await handler.HandleAsync(id, ct);
            return Results.Ok(published);
        })
        .WithName("PublishEvent")
        .WithTags("Events")
        .WithDescription("Publish a draft event")
        .Produces<Event>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id}", async (string id, Application.Interfaces.IEventRepository repository, CancellationToken ct) =>
        {
            await repository.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteEvent")
        .WithTags("Events")
        .WithDescription("Delete an event")
        .Produces(StatusCodes.Status204NoContent);

        return group;
    }
}

[Description("Request to create a new event")]
public record CreateEventRequest(
    [property: Description("Title of the event")]
    [property: JsonPropertyName("title")]
    string Title,

    [property: Description("Description of the event")]
    [property: JsonPropertyName("description")]
    string Description,

    [property: Description("Location where the event takes place")]
    [property: JsonPropertyName("location")]
    string Location,

    [property: Description("Start date and time of the event")]
    [property: JsonPropertyName("startDate")]
    DateTimeOffset StartDate,

    [property: Description("End date and time of the event")]
    [property: JsonPropertyName("endDate")]
    DateTimeOffset EndDate,

    [property: Description("Identifier of the event organizer")]
    [property: JsonPropertyName("organizerId")]
    string OrganizerId,

    [property: Description("Maximum number of attendees")]
    [property: JsonPropertyName("maxAttendees")]
    int MaxAttendees,

    [property: Description("Optional URL for the event image")]
    [property: JsonPropertyName("imageUrl")]
    string? ImageUrl = null,

    [property: Description("Optional URL for the event website")]
    [property: JsonPropertyName("websiteUrl")]
    string? WebsiteUrl = null
);

[Description("Request to update an existing event")]
public record UpdateEventRequest(
    [property: Description("Title of the event")]
    [property: JsonPropertyName("title")]
    string Title,

    [property: Description("Description of the event")]
    [property: JsonPropertyName("description")]
    string Description,

    [property: Description("Location where the event takes place")]
    [property: JsonPropertyName("location")]
    string Location,

    [property: Description("Start date and time of the event")]
    [property: JsonPropertyName("startDate")]
    DateTimeOffset StartDate,

    [property: Description("End date and time of the event")]
    [property: JsonPropertyName("endDate")]
    DateTimeOffset EndDate,

    [property: Description("Identifier of the event organizer")]
    [property: JsonPropertyName("organizerId")]
    string OrganizerId,

    [property: Description("Maximum number of attendees")]
    [property: JsonPropertyName("maxAttendees")]
    int MaxAttendees,

    [property: Description("Optional URL for the event image")]
    [property: JsonPropertyName("imageUrl")]
    string? ImageUrl = null,

    [property: Description("Optional URL for the event website")]
    [property: JsonPropertyName("websiteUrl")]
    string? WebsiteUrl = null
);
