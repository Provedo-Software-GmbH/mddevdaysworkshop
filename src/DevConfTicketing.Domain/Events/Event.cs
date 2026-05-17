namespace DevConfTicketing.Domain.Events;

public class Event
{
    public required string Id { get; init; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public required DateTimeOffset StartDate { get; set; }
    public required DateTimeOffset EndDate { get; set; }
    public required string OrganizerId { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public string? ImageUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public int MaxAttendees { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
