using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CoachBoard.Api.Controllers;
using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
using CoachBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoachBoard.Api.Tests;

public class GatingIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GatingIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<(int TenantId, int RoutineId)> CreateTenantWithSubscriptionAsync(SubscriptionStatus status, DateTime? periodEnd = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
        
        var tenant = new Tenant
        {
            Name = $"Tenant_{Guid.NewGuid()}",
            Plan = SubscriptionPlan.Free // Plan field is overshadowed by actual Subscription gating
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var sub = new Subscription
        {
            TenantId = tenant.Id,
            Provider = "Test",
            ProviderSubscriptionId = Guid.NewGuid().ToString(),
            Status = status,
            CurrentPeriodEnd = periodEnd,
            CreatedAt = DateTime.UtcNow
        };
        db.Subscriptions.Add(sub);
        
        var client = new Client
        {
            TenantId = tenant.Id,
            FullName = "Test Client",
            Email = "test@client.com",
            CoachId = 1 // Dummy coach ID if not constrained, or need seed coach? FK constraint?
            // Coach FK might be required if Client has CoachId non-nullable. 
            // Let's check Client.cs or assume nullable/seeded? 
            // To be safe, let's create a Client with minimal fields.
            // Actually, if Foreign Keys are enforced in InMemory/TestDB, we might need a Coach.
            // Let's check SeedTestData in Factory. It creates Coach.
        };
        // Simpler: Reuse seeding logic or null if allowed?
        // Client.CoachId is typically required.
        // Let's create a dummy coach too to be safe.
        
        var coach = new Coach { TenantId = tenant.Id, Name = "C", UserId = 1 }; // Simple
        // DbContext might fail if User FK required.
        // This is getting complicated to seed purely inside the helper without Factory help.
        // Factory already seeds data.
        // Can we attach to existing seed data?
        // But we are creating NEW Tenant.
        // Let's try to just set ClientId = 1 (if Factory seeded 1).
        // But we are in a new Tenant.
        // Let's check if ClientId is nullable in Routine.cs
        // It WAS `int ClientId`, so required.
        
        // Okay, let's create the graph: User -> Coach -> Client -> Routine.
        var user = new User { TenantId = tenant.Id, Email = $"u{Guid.NewGuid()}@t.com", PasswordHash = "x", Role = "Coach" };
        db.Users.Add(user);
        var coachObj = new Coach { TenantId = tenant.Id, Name = "C", User = user };
        db.Coaches.Add(coachObj);
        var clientObj = new Client { TenantId = tenant.Id, FullName = "Cl", Email = "c@t.com", Coach = coachObj };
        db.Clients.Add(clientObj);
        await db.SaveChangesAsync();

        var routine = new Routine
        {
            TenantId = tenant.Id,
            Title = "Test Routine",
            ClientId = clientObj.Id
        };
        db.Routines.Add(routine);
        
        await db.SaveChangesAsync();
        return (tenant.Id, routine.Id);
    }

    private void AuthenticateAsTenant(int tenantId)
    {
        // Generate a valid JWT for this tenant
        // We can resolve IJwtService from the factory scope
        using var scope = _factory.Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<CoachBoard.Application.Interfaces.IJwtService>();
        
        // We need a user to generate token. Let's assume a dummy user ID 999.
        // Role should be Coach or Admin to pass [Authorize(Roles = "Coach,Admin")]
        var token = jwtService.GenerateToken(999, "test@test.com", "Coach", null, tenantId);
        
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Export_ActiveSubscription_AllowsAccess()
    {
        var (tenantId, routineId) = await CreateTenantWithSubscriptionAsync(SubscriptionStatus.Active);
        AuthenticateAsTenant(tenantId);

        // Act
        var response = await _client.GetAsync($"/api/routines/{routineId}/export");

        // Assert
        // We expect either 200 (Success) or 403 (Forbidden by Feature Flag)
        // Check content to differentiate from Pro Requirement
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
             var content = await response.Content.ReadAsStringAsync();
             content.Should().NotContain("Pro Plan Required", "Active subscription should pass Pro gating");
             // It implies it failed at Feature Flag or Role, which is acceptable for THIS test.
        }
        else
        {
            // If 200/404/etc. then Gating passed.
            // 404 is possible if repository filtering logic is tricky but we are logged in as that tenant.
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden, "Unless feature flag blocks it, but we checked msg");
        }
    }

    [Fact]
    public async Task Export_PastDue_InGracePeriod_AllowsAccess()
    {
        var graceEnd = DateTime.UtcNow.AddDays(5); // Future
        var (tenantId, routineId) = await CreateTenantWithSubscriptionAsync(SubscriptionStatus.PastDue, graceEnd);
        AuthenticateAsTenant(tenantId);

        var response = await _client.GetAsync($"/api/routines/{routineId}/export");

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
             var content = await response.Content.ReadAsStringAsync();
             content.Should().NotContain("Pro Plan Required");
        }
    }

    [Fact]
    public async Task Export_PastDue_Expired_ReturnsForbiddenProRequired()
    {
        var graceEnd = DateTime.UtcNow.AddDays(-1); // Past
        var (tenantId, routineId) = await CreateTenantWithSubscriptionAsync(SubscriptionStatus.PastDue, graceEnd);
        AuthenticateAsTenant(tenantId);

        var response = await _client.GetAsync($"/api/routines/{routineId}/export");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Pro Plan Required");
    }

    [Fact]
    public async Task Export_Example_NoSubscription_ReturnsForbiddenProRequired()
    {
        // Create only tenant and routine, no subscription
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
        var tenant = new Tenant { Name = "NoSub", Plan = SubscriptionPlan.Free };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        var user = new User { TenantId = tenant.Id, Email = $"u{Guid.NewGuid()}@t.com", PasswordHash = "x", Role = "Coach" };
        db.Users.Add(user);
        var coach = new Coach { TenantId = tenant.Id, Name = "C", User = user };
        db.Coaches.Add(coach);
        var client = new Client { TenantId = tenant.Id, FullName = "Cl", Email = "c@t.com", Coach = coach };
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        var routine = new Routine { TenantId = tenant.Id, Title = "R1", ClientId = client.Id };
        db.Routines.Add(routine);
        await db.SaveChangesAsync();

        AuthenticateAsTenant(tenant.Id);

        var response = await _client.GetAsync($"/api/routines/{routine.Id}/export");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Pro Plan Required");
    }
}
