using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Domain.Enums;
using CoachBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; // Required for Replace
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CoachBoard.Api.Tests;

public class BillingIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BillingIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        // We need to override IMercadoPagoClient for this test to avoid real calls or rely on stub logging
        // However, since we have a stub implementation, we can assert on the result directly.
        // But to be cleaner and verify integration with mocked behavior if desired, we can use WithWebHostBuilder.
        // For simplicity using the existing factory which uses the Stub.
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateProCheckout_AsTenantA_ReturnsUrlAndPersistsSubscription()
    {
        // Arrange
        var token = await _factory.GetUserATokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/billing/checkout/pro", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
        content.Should().NotBeNull();
        content!.InitPoint.Should().Contain("simulated=true"); // From stub

        // Verify DB persistence
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
        
        // Tenant A ID = 10 (from CustomWebApplicationFactory)
        var sub = db.Subscriptions
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefault(s => s.TenantId == 10 && s.Provider == "MercadoPago");

        sub.Should().NotBeNull();
        sub!.Status.Should().Be(SubscriptionStatus.Pending);
    }
}
