using DevConfTicketing.Application.Interfaces;
using DevConfTicketing.Domain.Events;
using DevConfTicketing.Domain.Tickets;

using Microsoft.Extensions.Logging;

namespace DevConfTicketing.Infrastructure;

public class SeedDataService(
    IEventRepository eventRepository,
    ITicketTypeRepository ticketTypeRepository,
    ITaxRateRepository taxRateRepository,
    ILogger<SeedDataService> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingEvents = await eventRepository.GetAllAsync(cancellationToken);
        if (existingEvents.Count > 0)
        {
            logger.LogInformation("Seed data already exists ({Count} events found). Skipping.", existingEvents.Count);
            return;
        }

        logger.LogInformation("Seeding development data...");

        await SeedTaxRatesAsync(cancellationToken);
        var events = await SeedEventsAsync(cancellationToken);
        await SeedTicketTypesAsync(events, cancellationToken);

        logger.LogInformation("Development seed data created successfully.");
    }

    private async Task SeedTaxRatesAsync(CancellationToken cancellationToken)
    {
        TaxRate[] taxRates =
        [
            new()
            {
                Id = "tax-de-standard",
                CountryCode = "DE",
                Name = "MwSt. 19%",
                Percentage = 19m,
                Description = "German standard VAT rate",
                IsDefault = true,
                IsActive = true
            },
            new()
            {
                Id = "tax-de-reduced",
                CountryCode = "DE",
                Name = "MwSt. 7%",
                Percentage = 7m,
                Description = "German reduced VAT rate (food, books, etc.)",
                IsDefault = false,
                IsActive = true
            },
            new()
            {
                Id = "tax-de-zero",
                CountryCode = "DE",
                Name = "Tax-free",
                Percentage = 0m,
                Description = "Tax-exempt items",
                IsDefault = false,
                IsActive = true
            }
        ];

        foreach (var taxRate in taxRates)
        {
            await taxRateRepository.CreateAsync(taxRate, cancellationToken);
        }

        logger.LogInformation("Seeded {Count} tax rates.", taxRates.Length);
    }

    private async Task<List<Event>> SeedEventsAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        Event[] events =
        [
            new()
            {
                Id = "evt-devdays-2026",
                Title = "MD DevDays 2026",
                Description = "The premier developer conference in Munich. Three days of talks, workshops, and networking with the best developers in the industry. Topics include AI, cloud-native, DevOps, and modern web development.",
                Location = "Alte Kongresshalle, Munich",
                StartDate = new DateTimeOffset(2026, 10, 14, 9, 0, 0, TimeSpan.FromHours(2)),
                EndDate = new DateTimeOffset(2026, 10, 16, 18, 0, 0, TimeSpan.FromHours(2)),
                OrganizerId = "sia-consulting",
                Status = EventStatus.Published,
                MaxAttendees = 500,
                ImageUrl = new Uri("https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=1200"),
                WebsiteUrl = new Uri("https://mddevdays.example.com"),
                CreatedAt = now.AddDays(-30),
                UpdatedAt = now.AddDays(-5)
            },
            new()
            {
                Id = "evt-cloud-summit",
                Title = "Cloud Native Summit 2026",
                Description = "A one-day deep dive into cloud-native technologies. Learn about Kubernetes, serverless, microservices, and infrastructure as code from industry experts.",
                Location = "Zenith Kulturhalle, Munich",
                StartDate = new DateTimeOffset(2026, 11, 20, 9, 0, 0, TimeSpan.FromHours(1)),
                EndDate = new DateTimeOffset(2026, 11, 20, 18, 0, 0, TimeSpan.FromHours(1)),
                OrganizerId = "sia-consulting",
                Status = EventStatus.Published,
                MaxAttendees = 300,
                ImageUrl = new Uri("https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=1200"),
                WebsiteUrl = new Uri("https://cloud-summit.example.com"),
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now.AddDays(-3)
            },
            new()
            {
                Id = "evt-ai-workshop",
                Title = "AI & ML Workshop Day",
                Description = "Hands-on workshop covering practical AI and machine learning applications. Build your own models, learn about LLMs, RAG patterns, and AI-powered development tools.",
                Location = "TechHub Munich, Werksviertel",
                StartDate = new DateTimeOffset(2026, 12, 5, 10, 0, 0, TimeSpan.FromHours(1)),
                EndDate = new DateTimeOffset(2026, 12, 5, 17, 0, 0, TimeSpan.FromHours(1)),
                OrganizerId = "sia-consulting",
                Status = EventStatus.Draft,
                MaxAttendees = 50,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-5)
            }
        ];

        var createdEvents = new List<Event>();
        foreach (var @event in events)
        {
            createdEvents.Add(await eventRepository.CreateAsync(@event, cancellationToken));
        }

        logger.LogInformation("Seeded {Count} events.", events.Length);
        return createdEvents;
    }

    private async Task SeedTicketTypesAsync(List<Event> events, CancellationToken cancellationToken)
    {
        var count = 0;

        // MD DevDays 2026 ticket types
        var devDaysEvent = events.Find(e => e.Id == "evt-devdays-2026");
        if (devDaysEvent is not null)
        {
            TicketType[] devDaysTickets =
            [
                new()
                {
                    Id = "tt-devdays-full",
                    EventId = devDaysEvent.Id,
                    Name = "Full Conference Pass",
                    Description = "Access to all three days including talks, workshops, and networking events",
                    Price = 499.00m,
                    Currency = "EUR",
                    AvailableQuantity = 300,
                    SoldQuantity = 42,
                    MaxPerOrder = 5,
                    ShowRemainingQuantity = true,
                    SaleStart = DateTimeOffset.UtcNow.AddDays(-14),
                    SaleEnd = new DateTimeOffset(2026, 10, 13, 23, 59, 59, TimeSpan.FromHours(2)),
                    LineItems =
                    [
                        new LineItemTemplate
                        {
                            Name = "Conference Ticket",
                            NetAmount = 399.16m,
                            TaxRateId = "tax-de-standard",
                            TaxRateName = "MwSt. 19%",
                            TaxRatePercentage = 19m
                        },
                        new LineItemTemplate
                        {
                            Name = "Catering (3 days)",
                            NetAmount = 20.56m,
                            TaxRateId = "tax-de-reduced",
                            TaxRateName = "MwSt. 7%",
                            TaxRatePercentage = 7m
                        }
                    ]
                },
                new()
                {
                    Id = "tt-devdays-single",
                    EventId = devDaysEvent.Id,
                    Name = "Single Day Pass",
                    Description = "Access to one day of your choice with all talks and workshops",
                    Price = 199.00m,
                    Currency = "EUR",
                    AvailableQuantity = 200,
                    SoldQuantity = 15,
                    MaxPerOrder = 5,
                    ShowRemainingQuantity = false,
                    SaleStart = DateTimeOffset.UtcNow.AddDays(-14),
                    SaleEnd = new DateTimeOffset(2026, 10, 13, 23, 59, 59, TimeSpan.FromHours(2)),
                    LineItems =
                    [
                        new LineItemTemplate
                        {
                            Name = "Day Pass",
                            NetAmount = 160.50m,
                            TaxRateId = "tax-de-standard",
                            TaxRateName = "MwSt. 19%",
                            TaxRatePercentage = 19m
                        },
                        new LineItemTemplate
                        {
                            Name = "Catering (1 day)",
                            NetAmount = 6.54m,
                            TaxRateId = "tax-de-reduced",
                            TaxRateName = "MwSt. 7%",
                            TaxRatePercentage = 7m
                        }
                    ]
                },
                new()
                {
                    Id = "tt-devdays-student",
                    EventId = devDaysEvent.Id,
                    Name = "Student Pass",
                    Description = "Discounted full conference pass for students (valid student ID required at check-in)",
                    Price = 99.00m,
                    Currency = "EUR",
                    AvailableQuantity = 50,
                    SoldQuantity = 8,
                    MaxPerOrder = 1,
                    ShowRemainingQuantity = true,
                    SaleStart = DateTimeOffset.UtcNow.AddDays(-14),
                    SaleEnd = new DateTimeOffset(2026, 10, 13, 23, 59, 59, TimeSpan.FromHours(2)),
                    LineItems =
                    [
                        new LineItemTemplate
                        {
                            Name = "Student Ticket",
                            NetAmount = 83.19m,
                            TaxRateId = "tax-de-standard",
                            TaxRateName = "MwSt. 19%",
                            TaxRatePercentage = 19m
                        }
                    ]
                }
            ];

            foreach (var ticket in devDaysTickets)
            {
                await ticketTypeRepository.CreateAsync(ticket, cancellationToken);
                count++;
            }
        }

        // Cloud Native Summit ticket types
        var cloudEvent = events.Find(e => e.Id == "evt-cloud-summit");
        if (cloudEvent is not null)
        {
            TicketType[] cloudTickets =
            [
                new()
                {
                    Id = "tt-cloud-standard",
                    EventId = cloudEvent.Id,
                    Name = "Standard Ticket",
                    Description = "Full day access to all sessions and networking",
                    Price = 249.00m,
                    Currency = "EUR",
                    AvailableQuantity = 250,
                    SoldQuantity = 0,
                    MaxPerOrder = 10,
                    ShowRemainingQuantity = false,
                    LineItems =
                    [
                        new LineItemTemplate
                        {
                            Name = "Conference Access",
                            NetAmount = 202.52m,
                            TaxRateId = "tax-de-standard",
                            TaxRateName = "MwSt. 19%",
                            TaxRatePercentage = 19m
                        },
                        new LineItemTemplate
                        {
                            Name = "Lunch & Refreshments",
                            NetAmount = 7.48m,
                            TaxRateId = "tax-de-reduced",
                            TaxRateName = "MwSt. 7%",
                            TaxRatePercentage = 7m
                        }
                    ]
                },
                new()
                {
                    Id = "tt-cloud-vip",
                    EventId = cloudEvent.Id,
                    Name = "VIP Ticket",
                    Description = "Priority seating, speaker dinner, and exclusive workshop access",
                    Price = 449.00m,
                    Currency = "EUR",
                    AvailableQuantity = 50,
                    SoldQuantity = 0,
                    MaxPerOrder = 3,
                    ShowRemainingQuantity = true,
                    LineItems =
                    [
                        new LineItemTemplate
                        {
                            Name = "VIP Conference Access",
                            NetAmount = 369.75m,
                            TaxRateId = "tax-de-standard",
                            TaxRateName = "MwSt. 19%",
                            TaxRatePercentage = 19m
                        },
                        new LineItemTemplate
                        {
                            Name = "VIP Catering & Dinner",
                            NetAmount = 9.35m,
                            TaxRateId = "tax-de-reduced",
                            TaxRateName = "MwSt. 7%",
                            TaxRatePercentage = 7m
                        }
                    ]
                }
            ];

            foreach (var ticket in cloudTickets)
            {
                await ticketTypeRepository.CreateAsync(ticket, cancellationToken);
                count++;
            }
        }

        // AI Workshop ticket types
        var aiEvent = events.Find(e => e.Id == "evt-ai-workshop");
        if (aiEvent is not null)
        {
            TicketType[] aiTickets =
            [
                new()
                {
                    Id = "tt-ai-workshop",
                    EventId = aiEvent.Id,
                    Name = "Workshop Ticket",
                    Description = "Full day hands-on workshop including materials and lunch",
                    Price = 349.00m,
                    Currency = "EUR",
                    AvailableQuantity = 50,
                    SoldQuantity = 0,
                    MaxPerOrder = 2,
                    ShowRemainingQuantity = true,
                    LineItems =
                    [
                        new LineItemTemplate
                        {
                            Name = "Workshop Access",
                            NetAmount = 285.71m,
                            TaxRateId = "tax-de-standard",
                            TaxRateName = "MwSt. 19%",
                            TaxRatePercentage = 19m
                        },
                        new LineItemTemplate
                        {
                            Name = "Materials & Lunch",
                            NetAmount = 7.48m,
                            TaxRateId = "tax-de-reduced",
                            TaxRateName = "MwSt. 7%",
                            TaxRatePercentage = 7m
                        }
                    ]
                }
            ];

            foreach (var ticket in aiTickets)
            {
                await ticketTypeRepository.CreateAsync(ticket, cancellationToken);
                count++;
            }
        }

        logger.LogInformation("Seeded {Count} ticket types.", count);
    }
}
