using System.ComponentModel;
using System.Text.Json.Serialization;

using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Application.Orders;
using DevConfTicketing.Domain.Orders;

namespace DevConfTicketing.Api.Endpoints;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (string eventId, IOrderRepository repository, CancellationToken ct) =>
        {
            var orders = await repository.GetByEventIdAsync(eventId, ct);
            return Results.Ok(orders);
        })
        .WithName("GetOrders")
        .WithTags("Orders")
        .WithDescription("Get all orders for an event")
        .Produces<IReadOnlyList<Order>>()
        .RequireAuthorization("AdminPolicy");

        group.MapGet("/{id}", async (string eventId, string id, IOrderRepository repository, CancellationToken ct) =>
        {
            var order = await repository.GetByIdAsync(eventId, id, ct);
            return order is not null ? Results.Ok(order) : Results.NotFound();
        })
        .WithName("GetOrderById")
        .WithTags("Orders")
        .WithDescription("Get an order by ID")
        .Produces<Order>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization("AdminPolicy");

        group.MapPost("/", async (string eventId, CreateOrderRequest request, CreateOrderHandler handler, CancellationToken ct) =>
        {
            var order = await handler.HandleAsync(
                eventId,
                request.CustomerEmail,
                request.CustomerName,
                new Dictionary<string, int>(request.TicketSelections),
                request.VoucherCode,
                ct);

            return Results.Created($"/api/v1/events/{eventId}/orders/{order.Id}", order);
        })
        .WithName("CreateOrder")
        .WithTags("Orders")
        .WithDescription("Create a new order for an event")
        .Produces<Order>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        return group;
    }
}

[Description("Request to create a new order")]
public record CreateOrderRequest(
    [property: Description("Email address of the customer")]
    [property: JsonPropertyName("customerEmail")]
    string CustomerEmail,

    [property: Description("Full name of the customer")]
    [property: JsonPropertyName("customerName")]
    string? CustomerName,

    [property: Description("Dictionary of ticket type IDs to quantities")]
    [property: JsonPropertyName("ticketSelections")]
    IReadOnlyDictionary<string, int> TicketSelections,

    [property: Description("Optional voucher code to apply")]
    [property: JsonPropertyName("voucherCode")]
    string? VoucherCode = null
);
