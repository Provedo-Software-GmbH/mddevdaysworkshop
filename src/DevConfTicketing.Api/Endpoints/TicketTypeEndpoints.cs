using System.ComponentModel;
using System.Text.Json.Serialization;

using DevConfTicketing.Application.Tickets;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Api.Endpoints;

public static class TicketTypeEndpoints
{
    public static RouteGroupBuilder MapTicketTypeEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (string eventId, GetTicketTypesHandler handler, CancellationToken ct) =>
        {
            var ticketTypes = await handler.GetByEventIdAsync(eventId, ct);
            return Results.Ok(ticketTypes);
        })
        .WithName("GetTicketTypes")
        .WithTags("TicketTypes")
        .WithDescription("Get all ticket types for an event")
        .Produces<IReadOnlyList<TicketType>>();

        group.MapGet("/{id}", async (string eventId, string id, GetTicketTypesHandler handler, CancellationToken ct) =>
        {
            var ticketType = await handler.GetByIdAsync(eventId, id, ct);
            return ticketType is not null ? Results.Ok(ticketType) : Results.NotFound();
        })
        .WithName("GetTicketTypeById")
        .WithTags("TicketTypes")
        .WithDescription("Get a ticket type by ID")
        .Produces<TicketType>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (string eventId, CreateTicketTypeRequest request, CreateTicketTypeHandler handler, CancellationToken ct) =>
        {
            var ticketType = new TicketType
            {
                Id = string.Empty,
                EventId = eventId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency,
                AvailableQuantity = request.AvailableQuantity,
                MaxPerOrder = request.MaxPerOrder,
                ShowRemainingQuantity = request.ShowRemainingQuantity,
                SaleStart = request.SaleStart,
                SaleEnd = request.SaleEnd,
                LineItems = request.LineItems.ToList()
            };

            var created = await handler.HandleAsync(eventId, ticketType, ct);
            return Results.Created($"/api/v1/events/{eventId}/ticket-types/{created.Id}", created);
        })
        .WithName("CreateTicketType")
        .WithTags("TicketTypes")
        .WithDescription("Create a new ticket type for an event")
        .Produces<TicketType>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .RequireAuthorization("AdminPolicy");

        group.MapPut("/{id}", async (string eventId, string id, UpdateTicketTypeRequest request, UpdateTicketTypeHandler handler, CancellationToken ct) =>
        {
            var ticketType = new TicketType
            {
                Id = id,
                EventId = eventId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency,
                AvailableQuantity = request.AvailableQuantity,
                MaxPerOrder = request.MaxPerOrder,
                ShowRemainingQuantity = request.ShowRemainingQuantity,
                SaleStart = request.SaleStart,
                SaleEnd = request.SaleEnd,
                LineItems = request.LineItems.ToList()
            };

            var updated = await handler.HandleAsync(eventId, id, ticketType, ct);
            return Results.Ok(updated);
        })
        .WithName("UpdateTicketType")
        .WithTags("TicketTypes")
        .WithDescription("Update an existing ticket type")
        .Produces<TicketType>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization("AdminPolicy");

        group.MapDelete("/{id}", async (string eventId, string id, DeleteTicketTypeHandler handler, CancellationToken ct) =>
        {
            await handler.HandleAsync(eventId, id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteTicketType")
        .WithTags("TicketTypes")
        .WithDescription("Delete a ticket type")
        .Produces(StatusCodes.Status204NoContent)
        .RequireAuthorization("AdminPolicy");

        return group;
    }
}

[Description("Request to create a new ticket type")]
public record CreateTicketTypeRequest(
    [property: Description("Name of the ticket type")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Description("Description of the ticket type")]
    [property: JsonPropertyName("description")]
    string Description,

    [property: Description("Total price of the ticket")]
    [property: JsonPropertyName("price")]
    decimal Price,

    [property: Description("Currency code")]
    [property: JsonPropertyName("currency")]
    string Currency,

    [property: Description("Number of tickets available")]
    [property: JsonPropertyName("availableQuantity")]
    int AvailableQuantity,

    [property: Description("Tax-relevant line item breakdown")]
    [property: JsonPropertyName("lineItems")]
    IReadOnlyList<LineItemTemplate> LineItems,

    [property: Description("Maximum tickets per order")]
    [property: JsonPropertyName("maxPerOrder")]
    int MaxPerOrder = 10,

    [property: Description("Whether to show remaining quantity")]
    [property: JsonPropertyName("showRemainingQuantity")]
    bool ShowRemainingQuantity = false,

    [property: Description("Start of the sale window")]
    [property: JsonPropertyName("saleStart")]
    DateTimeOffset? SaleStart = null,

    [property: Description("End of the sale window")]
    [property: JsonPropertyName("saleEnd")]
    DateTimeOffset? SaleEnd = null
);

[Description("Request to update an existing ticket type")]
public record UpdateTicketTypeRequest(
    [property: Description("Name of the ticket type")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Description("Description of the ticket type")]
    [property: JsonPropertyName("description")]
    string Description,

    [property: Description("Total price of the ticket")]
    [property: JsonPropertyName("price")]
    decimal Price,

    [property: Description("Currency code")]
    [property: JsonPropertyName("currency")]
    string Currency,

    [property: Description("Number of tickets available")]
    [property: JsonPropertyName("availableQuantity")]
    int AvailableQuantity,

    [property: Description("Tax-relevant line item breakdown")]
    [property: JsonPropertyName("lineItems")]
    IReadOnlyList<LineItemTemplate> LineItems,

    [property: Description("Maximum tickets per order")]
    [property: JsonPropertyName("maxPerOrder")]
    int MaxPerOrder = 10,

    [property: Description("Whether to show remaining quantity")]
    [property: JsonPropertyName("showRemainingQuantity")]
    bool ShowRemainingQuantity = false,

    [property: Description("Start of the sale window")]
    [property: JsonPropertyName("saleStart")]
    DateTimeOffset? SaleStart = null,

    [property: Description("End of the sale window")]
    [property: JsonPropertyName("saleEnd")]
    DateTimeOffset? SaleEnd = null
);
