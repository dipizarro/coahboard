using System.Net;
using System.Net.Http.Json;
using CoachBoard.Application.DTOs;
using CoachBoard.Domain.Entities;
using FluentAssertions;

namespace CoachBoard.Api.Tests;

public class TenantIsolationIntegrationTests : BaseIntegrationTest
{
    public TenantIsolationIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetClient_OwnedByOtherTenant_ReturnsNotFound()
    {
        // Arrange
        // Seed a client for Tenant B (User B)
        var clientB = new Client 
        { 
            FullName = "Client B", 
            Email = "clientB@test.local", 
            Phone = "123", 
            CoachId = 11, 
            TenantId = 11 
        };
        Db.Clients.Add(clientB);
        await Db.SaveChangesAsync();

        // Act
        // Login as User A (Tenant A)
        await AuthenticateAsUserAAsync();
        
        // Try to access Client B
        var response = await Client.GetAsync($"/api/clients/{clientB.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetClients_OnlyReturnsTenantsOwnClients()
    {
        // Arrange
        var clientA = new Client { FullName = "Client A", Email = "a@test.local", Phone = "1", CoachId = 10, TenantId = 10 };
        var clientB = new Client { FullName = "Client B", Email = "b@test.local", Phone = "2", CoachId = 11, TenantId = 11 };
        
        Db.Clients.AddRange(clientA, clientB);
        await Db.SaveChangesAsync();

        // Act
        await AuthenticateAsUserAAsync();
        var response = await Client.GetAsync("/api/clients?coachId=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<dynamic>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
    }
}

public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
