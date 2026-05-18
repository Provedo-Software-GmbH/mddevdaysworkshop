using DevConfTicketing.Api.Endpoints;
using DevConfTicketing.Api.Middleware;
using DevConfTicketing.Application.Events;
using DevConfTicketing.Application.Orders;
using DevConfTicketing.Application.Tickets;
using DevConfTicketing.Infrastructure;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// Authentication — Microsoft Entra ID (JWT Bearer)
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

// Authorization policies — roles defined in Entra ID App Registration
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin", "EventManager"))
    .AddPolicy("EventManagerPolicy", policy =>
        policy.RequireRole("EventManager", "Admin"));

builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// Application layer handlers
builder.Services.AddScoped<CreateEventHandler>();
builder.Services.AddScoped<UpdateEventHandler>();
builder.Services.AddScoped<GetEventsHandler>();
builder.Services.AddScoped<PublishEventHandler>();
builder.Services.AddScoped<CreateTicketTypeHandler>();
builder.Services.AddScoped<GetTicketTypesHandler>();
builder.Services.AddScoped<UpdateTicketTypeHandler>();
builder.Services.AddScoped<DeleteTicketTypeHandler>();
builder.Services.AddScoped<CreateTaxRateHandler>();
builder.Services.AddScoped<GetTaxRatesHandler>();
builder.Services.AddScoped<UpdateTaxRateHandler>();
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<TaxCalculationService>();
builder.Services.AddScoped<VoucherValidationService>();

// CORS — allow traceparent header for end-to-end distributed tracing with the frontend
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins([.. corsOrigins, "http://localhost:5173"])
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("X-Correlation-ID")
              .WithHeaders("Content-Type", "Authorization", "traceparent", "tracestate", "X-Correlation-ID");
    });
});

var app = builder.Build();

// Initialize Cosmos DB containers and seed data in development
if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeCosmosDbAsync();

    using var scope = app.Services.CreateScope();
    var seedDataService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
    await seedDataService.SeedAsync();
}

// Middleware pipeline (order matters)
app.UseMiddleware<RequestTelemetryMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Health check
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTimeOffset.UtcNow }))
   .WithName("HealthCheck")
   .WithTags("Health")
   .WithDescription("Health check endpoint");

// Frontend telemetry proxy endpoint
app.MapTelemetryEndpoints();

// API endpoint groups
app.MapGroup("/api/v1/events").MapEventEndpoints();
app.MapGroup("/api/v1/events/{eventId}/ticket-types").MapTicketTypeEndpoints();
app.MapGroup("/api/v1/tax-rates").MapTaxRateEndpoints();
app.MapGroup("/api/v1/events/{eventId}/orders").MapOrderEndpoints();
app.MapGroup("/api/v1/events/{eventId}/orders").MapPaymentEndpoints();
app.MapWebhookEndpoints();
app.MapVoucherEndpoints();

app.Run();
