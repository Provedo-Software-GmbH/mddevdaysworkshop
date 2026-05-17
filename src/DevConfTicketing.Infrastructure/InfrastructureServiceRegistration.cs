using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;

using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Infrastructure.Cosmos;
using DevConfTicketing.Infrastructure.Cosmos.Repositories;
using DevConfTicketing.Infrastructure.Monitoring;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Resources;

namespace DevConfTicketing.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Cosmos DB
        var accountEndpoint = configuration["CosmosDb:AccountEndpoint"];
        var databaseName = configuration["CosmosDb:DatabaseName"] ?? "devconf-ticketing";

        services.AddSingleton(sp =>
        {
            CosmosClientOptions clientOptions = new()
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };

            if (environment.IsDevelopment() && accountEndpoint == "https://localhost:8081")
            {
                // Well-known Cosmos DB emulator key — this is a publicly documented credential, not a secret
                const string cosmosEmulatorKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
                return new CosmosClient(accountEndpoint, cosmosEmulatorKey, clientOptions);
            }

            return new CosmosClient(accountEndpoint, new DefaultAzureCredential(), clientOptions);
        });

        services.AddSingleton(sp =>
        {
            var cosmosClient = sp.GetRequiredService<CosmosClient>();
            var logger = sp.GetRequiredService<ILogger<CosmosDbService>>();
            return new CosmosDbService(cosmosClient, databaseName, logger);
        });

        // Repositories
        services.AddSingleton<IEventRepository, EventRepository>();
        services.AddSingleton<ITicketTypeRepository, TicketTypeRepository>();
        services.AddSingleton<ITaxRateRepository, TaxRateRepository>();
        services.AddSingleton<IOrderRepository, OrderRepository>();
        services.AddSingleton<IVoucherRepository, VoucherRepository>();

        // OpenTelemetry with Azure Monitor exporter
        // Uses System.Diagnostics.Activity for distributed tracing and System.Diagnostics.Metrics for metrics.
        // Application Insights is a passive collector via the Azure Monitor exporter.
        var connectionString = configuration["ApplicationInsights:ConnectionString"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: "DevConfTicketing",
                serviceVersion: typeof(InfrastructureServiceRegistration).Assembly
                    .GetName().Version?.ToString() ?? "0.0.0",
                serviceNamespace: "devconf-ticketing"))
            .WithTracing(tracing => tracing
                .AddSource(TelemetryService.ActivitySourceName))
            .WithMetrics(metrics => metrics
                .AddMeter(TelemetryService.MeterName));

        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = connectionString;
            });
        }
        else
        {
            // In development without a connection string, Azure Monitor exporter is skipped.
            // Traces and metrics are still collected via the OpenTelemetry SDK for local diagnostics.
            services.AddOpenTelemetry().UseAzureMonitor();
        }

        services.AddSingleton<ITelemetryService, TelemetryService>();

        return services;
    }

    public static async Task InitializeCosmosDbAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var cosmosDbService = serviceProvider.GetRequiredService<CosmosDbService>();

        await cosmosDbService.EnsureDatabaseAsync(cancellationToken);

        CosmosContainerConfig[] containers =
        [
            EventRepository.ContainerConfig,
            TicketTypeRepository.ContainerConfig,
            TaxRateRepository.ContainerConfig,
            OrderRepository.ContainerConfig,
            VoucherRepository.ContainerConfig
        ];

        foreach (var container in containers)
        {
            await cosmosDbService.EnsureContainerAsync(container, cancellationToken);
        }
    }
}
