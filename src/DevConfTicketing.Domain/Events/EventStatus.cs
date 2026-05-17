using System.ComponentModel;

namespace DevConfTicketing.Domain.Events;

[Description("Represents the lifecycle status of an event")]
public enum EventStatus
{
    [Description("Event is in draft and not yet visible")]
    Draft,

    [Description("Event is published and visible to attendees")]
    Published,

    [Description("Event has been cancelled")]
    Cancelled,

    [Description("Event has been archived and is no longer active")]
    Archived
}
