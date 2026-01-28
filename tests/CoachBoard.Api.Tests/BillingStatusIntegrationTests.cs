using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CoachBoard.Application.DTOs;
using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
using CoachBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoachBoard.Api.Tests;

public class BillingStatusIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BillingStatusIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<int> CreateTenantWithSubscriptionAsync(SubscriptionStatus status, DateTime? periodEnd = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
        
        var tenant = new Tenant
        {
            Name = $"Tenant_Status_{Guid.NewGuid()}",
            Plan = SubscriptionPlan.Free
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        if (status != SubscriptionStatus.None)
        {
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
            await db.SaveChangesAsync();
        }
        
        return tenant.Id;
    }

    private void AuthenticateAsTenant(int tenantId)
    {
        using var scope = _factory.Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<CoachBoard.Application.Interfaces.IJwtService>();
        var token = jwtService.GenerateToken(999, "test@test.com", "Coach", null, tenantId);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetStatus_ActiveSubscription_ReturnsCorrectStatus()
    {
        var tenantId = await CreateTenantWithSubscriptionAsync(SubscriptionStatus.Active);
        AuthenticateAsTenant(tenantId);

        var response = await _client.GetAsync("/api/billing/status");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BillingStatusDto>();
        
        result.Should().NotBeNull();
        result!.CanAccessPro.Should().BeTrue();
        result.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task GetStatus_PastDueInGrace_ReturnsCorrectStatus()
    {
        var graceEnd = DateTime.UtcNow.AddDays(5);
        var tenantId = await CreateTenantWithSubscriptionAsync(SubscriptionStatus.PastDue, graceEnd);
        AuthenticateAsTenant(tenantId);

        var response = await _client.GetAsync("/api/billing/status");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BillingStatusDto>();
        
        result.Should().NotBeNull();
        result!.CanAccessPro.Should().BeTrue();
        result.SubscriptionStatus.Should().Be(SubscriptionStatus.PastDue);
        result.CurrentPeriodEnd.Should().BeCloseTo(graceEnd, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetStatus_Expired_ReturnsCorrectStatus()
    {
        var graceEnd = DateTime.UtcNow.AddDays(-1);
        var tenantId = await CreateTenantWithSubscriptionAsync(SubscriptionStatus.PastDue, graceEnd);
        AuthenticateAsTenant(tenantId);

        var response = await _client.GetAsync("/api/billing/status");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BillingStatusDto>();
        
        result.Should().NotBeNull();
        result!.CanAccessPro.Should().BeFalse();
        result.SubscriptionStatus.Should().Be(SubscriptionStatus.PastDue); // Status is PastDue, but access denied
    }

    [Fact]
    public async Task GetStatus_NoSubscription_ReturnsNone()
    {
        var tenantId = await CreateTenantWithSubscriptionAsync(SubscriptionStatus.None);
        AuthenticateAsTenant(tenantId);

        var response = await _client.GetAsync("/api/billing/status");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BillingStatusDto>();
        
        result.Should().NotBeNull();
        result!.CanAccessPro.Should().BeFalse();
        result.SubscriptionStatus.Should().Be(SubscriptionStatus.None);
    }
}
