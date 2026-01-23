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
        result!.Items.Should().HaveCount(2);
    }
    [Fact]
    public async Task Routine_TenantIsolation_FullScenario()
    {
        // 1. Authenticate as User A
        await AuthenticateAsUserAAsync();

        // 2. Create a Routine for Client 10 (Tenant A) using Exercise 10 (Seeded in Factory)
        var createDto = new RoutineCreateDto(
            "Routine A", 
            10, 
            new List<RoutineItemDto> { new RoutineItemDto(10, 3, 10, 1, "Notes") }
        );
        var createResponse = await Client.PostAsJsonAsync("/api/routines", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var routine = await createResponse.Content.ReadFromJsonAsync<RoutineReadDto>();
        routine.Should().NotBeNull();

        // 3. Authenticate as User B
        await AuthenticateAsUserBAsync();

        // 4. Attempt to GET the Routine by ID -> Expect 404 Not Found (Isolation at Repo level)
        var getByIdResponse = await Client.GetAsync($"/api/routines/{routine!.Id}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 5. Attempt to GET all Routines for Client 10 -> Expect 403 Forbidden (Controller checks if Client belongs to Coach)
        // Note: Coach B (User B) doesn't own Client 10.
        var getByClientResponse = await Client.GetAsync("/api/routines?clientId=10");
        // Looking at RoutinesController.Get, there isn't an explicit "Coach B owns Client 10" check in the GET routines list?
        // Wait, RoutinesController.Get calls _repo.GetByClientAsync(clientId...).
        // But Client 10 belongs to Tenant 10, and User B is Tenant 11.
        // The repository tenant filter will apply, so User B (Tenant 11) won't find Client 10 if we have isolation there.
        // Actually, RoutinesController doesn't check ownership of ClientId manually like ClientsController does.
        // However, the RoutineRepository will filter Routines by TenantId 11, so it will return empty list.
        getByClientResponse.StatusCode.Should().Be(HttpStatusCode.OK); // List endpoint usually returns 200 even if empty
        var result = await getByClientResponse.Content.ReadFromJsonAsync<PagedResult<RoutineReadDto>>();
        result!.Items.Should().BeEmpty();

        // 6. Authenticate back as User A
        await AuthenticateAsUserAAsync();

        // 7. GET the Routine by ID -> Expect 200 OK
        var getByIdSuccessResponse = await Client.GetAsync($"/api/routines/{routine.Id}");
        getByIdSuccessResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalRoutine = await getByIdSuccessResponse.Content.ReadFromJsonAsync<RoutineReadDto>();
        finalRoutine!.Title.Should().Be("Routine A");
    }
}


