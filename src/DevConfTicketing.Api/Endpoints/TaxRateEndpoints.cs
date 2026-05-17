using System.ComponentModel;
using System.Text.Json.Serialization;

using DevConfTicketing.Application.Tickets;
using DevConfTicketing.Domain.Tickets;

namespace DevConfTicketing.Api.Endpoints;

public static class TaxRateEndpoints
{
    public static RouteGroupBuilder MapTaxRateEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (GetTaxRatesHandler handler, CancellationToken ct) =>
        {
            var taxRates = await handler.GetAllAsync(ct);
            return Results.Ok(taxRates);
        })
        .WithName("GetTaxRates")
        .WithTags("TaxRates")
        .WithDescription("Get all tax rates")
        .Produces<IReadOnlyList<TaxRate>>();

        group.MapGet("/country/{countryCode}", async (string countryCode, GetTaxRatesHandler handler, CancellationToken ct) =>
        {
            var taxRates = await handler.GetByCountryCodeAsync(countryCode, ct);
            return Results.Ok(taxRates);
        })
        .WithName("GetTaxRatesByCountry")
        .WithTags("TaxRates")
        .WithDescription("Get tax rates by country code")
        .Produces<IReadOnlyList<TaxRate>>();

        group.MapGet("/{countryCode}/{id}", async (string countryCode, string id, GetTaxRatesHandler handler, CancellationToken ct) =>
        {
            var taxRate = await handler.GetByIdAsync(countryCode, id, ct);
            return taxRate is not null ? Results.Ok(taxRate) : Results.NotFound();
        })
        .WithName("GetTaxRateById")
        .WithTags("TaxRates")
        .WithDescription("Get a tax rate by country code and ID")
        .Produces<TaxRate>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateTaxRateRequest request, CreateTaxRateHandler handler, CancellationToken ct) =>
        {
            var taxRate = new TaxRate
            {
                Id = string.Empty,
                CountryCode = request.CountryCode,
                Name = request.Name,
                Percentage = request.Percentage,
                Description = request.Description,
                IsDefault = request.IsDefault,
                IsActive = request.IsActive
            };

            var created = await handler.HandleAsync(taxRate, ct);
            return Results.Created($"/api/v1/tax-rates/{created.CountryCode}/{created.Id}", created);
        })
        .WithName("CreateTaxRate")
        .WithTags("TaxRates")
        .WithDescription("Create a new tax rate")
        .Produces<TaxRate>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapPut("/{countryCode}/{id}", async (string countryCode, string id, UpdateTaxRateRequest request, UpdateTaxRateHandler handler, CancellationToken ct) =>
        {
            var taxRate = new TaxRate
            {
                Id = id,
                CountryCode = countryCode,
                Name = request.Name,
                Percentage = request.Percentage,
                Description = request.Description,
                IsDefault = request.IsDefault,
                IsActive = request.IsActive
            };

            var updated = await handler.HandleAsync(countryCode, id, taxRate, ct);
            return Results.Ok(updated);
        })
        .WithName("UpdateTaxRate")
        .WithTags("TaxRates")
        .WithDescription("Update an existing tax rate")
        .Produces<TaxRate>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }
}

[Description("Request to create a new tax rate")]
public record CreateTaxRateRequest(
    [property: Description("Country code for the tax rate")]
    [property: JsonPropertyName("countryCode")]
    string CountryCode,

    [property: Description("Name of the tax rate")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Description("Tax percentage")]
    [property: JsonPropertyName("percentage")]
    decimal Percentage,

    [property: Description("Description of the tax rate")]
    [property: JsonPropertyName("description")]
    string Description,

    [property: Description("Whether this is the default tax rate for the country")]
    [property: JsonPropertyName("isDefault")]
    bool IsDefault = false,

    [property: Description("Whether the tax rate is active")]
    [property: JsonPropertyName("isActive")]
    bool IsActive = true
);

[Description("Request to update an existing tax rate")]
public record UpdateTaxRateRequest(
    [property: Description("Name of the tax rate")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Description("Tax percentage")]
    [property: JsonPropertyName("percentage")]
    decimal Percentage,

    [property: Description("Description of the tax rate")]
    [property: JsonPropertyName("description")]
    string Description,

    [property: Description("Whether this is the default tax rate for the country")]
    [property: JsonPropertyName("isDefault")]
    bool IsDefault = false,

    [property: Description("Whether the tax rate is active")]
    [property: JsonPropertyName("isActive")]
    bool IsActive = true
);
