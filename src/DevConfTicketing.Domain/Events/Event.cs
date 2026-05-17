using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DevConfTicketing.Domain.Events;

[Description("Represents a conference or developer event")]
public class Event
{
    [Description("Unique identifier of the event")]
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [Description("Title of the event")]
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [Description("Detailed description of the event")]
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [Description("Physical or virtual location of the event")]
    [JsonPropertyName("location")]
    public required string Location { get; set; }

    [Description("Date and time when the event starts")]
    [JsonPropertyName("startDate")]
    public required DateTimeOffset StartDate { get; set; }

    [Description("Date and time when the event ends")]
    [JsonPropertyName("endDate")]
    public required DateTimeOffset EndDate { get; set; }

    [Description("Identifier of the event organizer")]
    [JsonPropertyName("organizerId")]
    public required string OrganizerId { get; set; }

    [Description("Current status of the event")]
    [JsonPropertyName("status")]
    public EventStatus Status { get; set; } = EventStatus.Draft;

    [Description("URL of the event banner or logo image")]
    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [Description("URL of the event website")]
    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [Description("Maximum number of attendees allowed")]
    [JsonPropertyName("maxAttendees")]
    public int MaxAttendees { get; set; }

    [Description("Timestamp when the event was created")]
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; init; }

    [Description("Timestamp when the event was last updated")]
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}
