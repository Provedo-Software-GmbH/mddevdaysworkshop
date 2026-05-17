var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// Health check
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTimeOffset.UtcNow }))
   .WithTags("Health");

// API endpoint groups
app.MapGroup("/api/v1/events").MapEventEndpoints();
app.MapGroup("/api/v1/tax-rates").MapTaxRateEndpoints();

app.Run();

// Endpoint mapping extensions (placeholder implementations)
public static class EventEndpointExtensions
{
    public static RouteGroupBuilder MapEventEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", () => Results.Ok(Array.Empty<object>()))
             .WithTags("Events");

        group.MapGet("/{id}", (string id) => Results.NotFound())
             .WithTags("Events");

        return group;
    }
}

public static class TaxRateEndpointExtensions
{
    public static RouteGroupBuilder MapTaxRateEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", () => Results.Ok(Array.Empty<object>()))
             .WithTags("TaxRates");

        return group;
    }
}
