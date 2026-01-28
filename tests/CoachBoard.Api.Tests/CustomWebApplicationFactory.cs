using CoachBoard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Json;

namespace CoachBoard.Api.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly string _databaseName = $"CoachBoardTestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=CoachBoardTest;Trusted_Connection=True;",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:Key"] = "TestKey-TestKey-TestKey-TestKey-TestKey"
            };

            config.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContextOptions
            services.RemoveAll<DbContextOptions<CoachBoardDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<CoachBoardDbContext>();
            
            Console.WriteLine("Removed DbContext services");

            // Create options for InMemory DB
            var options = new DbContextOptionsBuilder<CoachBoardDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;

            // Register options explicitly to bypass checking IConfigureOptions (which would include the Program.cs SQL Server config)
            services.AddSingleton(options);

            // Register the context using these options
            services.AddScoped<CoachBoardDbContext>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
        db.Database.EnsureCreated();
        SeedTestData(db);
        return host;
    }

    private static void SeedTestData(CoachBoardDbContext db)
    {
        if (db.Users.Any(u => u.Email == "usera@test.local")) 
        {
            return;
        }

        // Global Exercise
        var exercise = new Domain.Entities.Exercise { Id = 10, Name = "Push Up", Category = "Chest" };

        // Seed Tenant A
        var tenantA = new Domain.Entities.Tenant { Id = 10, Name = "Tenant A", Plan = Domain.Enums.SubscriptionPlan.Free };
        var userA = new Domain.Entities.User
        {
            Id = 10,
            Email = "usera@test.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!", 11),
            Role = "Coach",
            TenantId = 10
        };
        var coachA = new Domain.Entities.Coach
        {
            Id = 10,
            UserId = 10,
            Name = "Coach A",
            Specialty = "General",
            TenantId = 10
        };
        var clientA = new Domain.Entities.Client
        {
            Id = 10,
            FullName = "Client A",
            Email = "clienta@test.local",
            CoachId = 10,
            TenantId = 10
        };

        // Seed Tenant B
        var tenantB = new Domain.Entities.Tenant { Id = 11, Name = "Tenant B", Plan = Domain.Enums.SubscriptionPlan.Free };
        var userB = new Domain.Entities.User
        {
            Id = 11,
            Email = "userb@test.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!", 11),
            Role = "Coach",
            TenantId = 11
        };
        var coachB = new Domain.Entities.Coach
        {
            Id = 11,
            UserId = 11,
            Name = "Coach B",
            Specialty = "General",
            TenantId = 11
        };
        var clientB = new Domain.Entities.Client
        {
            Id = 11,
            FullName = "Client B",
            Email = "clientb@test.local",
            CoachId = 11,
            TenantId = 11
        };

        db.Tenants.AddRange(tenantA, tenantB);
        db.Users.AddRange(userA, userB);
        db.Coaches.AddRange(coachA, coachB);
        db.Exercises.Add(exercise);
        db.Clients.AddRange(clientA, clientB);

        // Seed Subscriptions (Active for both to pass Pro Gating)
        var subA = new Domain.Entities.Subscription
        {
            TenantId = 10,
            Provider = "MP",
            ProviderSubscriptionId = "sub_A",
            Status = Domain.Enums.SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        var subB = new Domain.Entities.Subscription
        {
            TenantId = 11,
            Provider = "MP",
            ProviderSubscriptionId = "sub_B",
            Status = Domain.Enums.SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        db.Subscriptions.AddRange(subA, subB);

        // Seed Feature Flags
        var flagA = new Domain.Entities.FeatureFlag { Id = 1, Name = "feature.export_routine", IsEnabled = true, TenantId = 10 };
        // Tenant B will not have the flag, simulating disabled/missing
        
        db.FeatureFlags.Add(flagA);

        db.SaveChanges();
        Console.WriteLine($"Seeded userA: {userA.Email}, Hash: {userA.PasswordHash}");
    }

    public async Task<string> LoginAsAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login failed for {email}: {body}");
        }
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return result!.Token;
    }

    public Task<string> GetUserATokenAsync(HttpClient client) => LoginAsAsync(client, "usera@test.local", "P@ssw0rd!");
    public Task<string> GetUserBTokenAsync(HttpClient client) => LoginAsAsync(client, "userb@test.local", "P@ssw0rd!");
}

public record AuthResponse(string Token, string Email, string Role);
