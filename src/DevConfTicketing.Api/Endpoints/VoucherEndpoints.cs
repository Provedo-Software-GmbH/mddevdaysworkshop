using System.ComponentModel;
using System.Text.Json.Serialization;

using DevConfTicketing.Application.Orders;

namespace DevConfTicketing.Api.Endpoints;

public static class VoucherEndpoints
{
    public static IEndpointRouteBuilder MapVoucherEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/vouchers/validate", async (ValidateVoucherRequest request, VoucherValidationService voucherService, CancellationToken ct) =>
        {
            var result = await voucherService.ValidateAsync(request.EventId, request.Code, request.TicketTypeIds, ct);

            if (!result.IsValid)
            {
                return Results.Problem(
                    detail: result.ErrorMessage,
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }

            return Results.Ok(new ValidateVoucherResponse(
                result.IsValid,
                result.DiscountType?.ToString(),
                result.DiscountValue,
                result.ErrorMessage));
        })
        .WithName("ValidateVoucher")
        .WithTags("Vouchers")
        .WithDescription("Validates a voucher code and returns discount information if valid")
        .Produces<ValidateVoucherResponse>()
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return app;
    }
}

[Description("Request to validate a voucher code")]
public record ValidateVoucherRequest(
    [property: Description("Event ID the voucher should apply to")]
    [property: JsonPropertyName("eventId")]
    string EventId,

    [property: Description("Voucher code to validate")]
    [property: JsonPropertyName("code")]
    string Code,

    [property: Description("Ticket type IDs to check applicability against")]
    [property: JsonPropertyName("ticketTypeIds")]
    List<string>? TicketTypeIds = null
);

[Description("Response containing voucher validation result")]
public record ValidateVoucherResponse(
    [property: Description("Whether the voucher is valid")]
    [property: JsonPropertyName("isValid")]
    bool IsValid,

    [property: Description("Type of discount (Percentage or FixedAmount)")]
    [property: JsonPropertyName("discountType")]
    string? DiscountType,

    [property: Description("Discount value")]
    [property: JsonPropertyName("discountValue")]
    decimal? DiscountValue,

    [property: Description("Error message if validation failed")]
    [property: JsonPropertyName("errorMessage")]
    string? ErrorMessage
);
