using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CoachBoard.Api.Tests;

public class FeatureFlagEnforcementTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public FeatureFlagEnforcementTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Export_WhenFlagEnabled_Returns200()
    {
        // Tenant A (UserA) has "feature.export_routine" = true
        var token = await _factory.GetUserATokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Pre-create a routine for Tenant A
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
            // Ensure client and routine exist for Tenant A (Id=10)
            if (!context.Routines.Any(r => r.Title == "Routine For Export A"))
            {
                var routine = new Routine 
                { 
                    Title = "Routine For Export A", 
                    ClientId = 10, // Client A
                    TenantId = 10 
                };
                context.Routines.Add(routine);
                context.SaveChanges();
            }
        }

        // Get ID of created routine
        int routineId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
            routineId = context.Routines.First(r => r.Title == "Routine For Export A").Id;
        }

        // Act
        var response = await _client.GetAsync($"/api/routines/{routineId}/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Export for Routine");
    }

    [Fact]
    public async Task Export_WhenFlagDisabledOrMissing_Returns403()
    {
        // Tenant B (UserB) does NOT have "feature.export_routine" (defaults to false)
        var token = await _factory.GetUserBTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Pre-create a routine for Tenant B
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
            // Ensure client and routine exist for Tenant B (Id=11)
            if (!context.Routines.Any(r => r.Title == "Routine For Export B"))
            {
                var routine = new Routine 
                { 
                    Title = "Routine For Export B", 
                    ClientId = 11, // Client B
                    TenantId = 11 
                };
                context.Routines.Add(routine);
                context.SaveChanges();
            }
        }

        // Get ID of created routine
        int routineId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CoachBoardDbContext>();
            routineId = context.Routines.First(r => r.Title == "Routine For Export B").Id;
        }

        // Act
        var response = await _client.GetAsync($"/api/routines/{routineId}/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
